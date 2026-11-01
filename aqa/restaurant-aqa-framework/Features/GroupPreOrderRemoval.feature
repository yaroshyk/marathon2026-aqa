@group-preorder @L3
Feature: Removing a participant from the group pre-order

  The owner can remove a guest and their cart from the group pre-order. Afterwards the
  owner sees the updated list and the new total. Only a guest who belongs to this group
  and exists can be removed, and only by the owner.

  Background:
    Given the owner has booked a table for a group of 5

  Rule: The owner removes a guest and the totals follow

    @ui
    Scenario: The owner removes one guest of several
      Given these guests joined the group pre-order:
        | guest | dishes                 | cart      |
        | Anna  | 2 x dish A             | submitted |
        | Boris | 1 x dish A, 1 x dish B | submitted |
        | Clara | 1 x dish C             | submitted |
      And the owner opens the group pre-order
      When the owner removes Boris from the group pre-order
      Then Boris is no longer listed
      And the owner sees every guest with their dishes, quantities and subtotal
      And the grand total is the sum of the submitted carts

    @api
    Scenario: The service recalculates the total without the removed guest
      Given these guests joined the group pre-order:
        | guest | dishes                 | cart           |
        | Anna  | 2 x dish A             | submitted      |
        | Boris | 1 x dish A, 1 x dish B | still choosing |
        | Clara | 1 x dish C             | submitted      |
      When the owner removes Boris from the group pre-order
      Then Boris is no longer listed
      And the owner sees every guest with their dishes, quantities and subtotal
      And the group total is the sum of every participant's total

    @api
    Scenario: The owner removes a guest who has already submitted their cart
      Given these guests joined the group pre-order:
        | guest | dishes     | cart           |
        | Anna  | 2 x dish A | submitted      |
        | Clara | 1 x dish C | still choosing |
      When the owner removes Anna from the group pre-order
      Then Anna is no longer listed
      And the owner sees every guest with their dishes, quantities and subtotal

    @api
    Scenario: The owner removes the last guest
      Given these guests joined the group pre-order:
        | guest | dishes     | cart      |
        | Anna  | 2 x dish A | submitted |
      When the owner removes Anna from the group pre-order
      Then nobody is listed and the group total is zero

  Rule: Only a guest of this group can be removed

    @api @negative
    Scenario: The owner tries to remove a guest of another group
      Given guest Anna joined the group pre-order
      And the owner has another group reservation that Zoe joined
      When the owner removes Zoe from the group pre-order
      Then the owner is told "Guest session does not belong to this group reservation."
      And Anna is still listed

    @api @negative
    Scenario: The owner tries to remove a guest who does not exist
      Given guest Anna joined the group pre-order
      When the owner removes a guest who does not exist
      Then the owner is told that the guest was not found

  Rule: Only the owner removes guests

    @api @negative
    Scenario Outline: Someone who is not the owner cannot remove a guest
      Given guest Anna joined the group pre-order
      When <someone> removes Anna from the owner's group pre-order
      Then <outcome>
      And Anna is still listed

      Examples: Signed in, but not the owner
        | someone          | outcome                                    |
        | another customer | they are refused as they are not the owner |
        | a waiter         | they are refused as they are not the owner |

      Examples: Not signed in
        | someone   | outcome                   |
        | a visitor | they are asked to sign in |
