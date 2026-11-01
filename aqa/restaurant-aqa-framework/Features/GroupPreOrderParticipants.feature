@group-preorder @L1
Feature: Group pre-order participants

  The owner of a group reservation shares an invite link. Every guest who opens it
  builds their own cart, and the owner sees who joined, what each guest chose, how
  many of each dish and what each cart costs. Dishes are named "dish A", "dish B", …:
  the first, second, … dish on the menu of the booked table's location.

  Background:
    Given the owner has booked a table for a group of 5

  Rule: The owner sees every participant with their dishes and totals

    @ui @smoke
    Scenario: The owner sees each guest's dishes, quantities and subtotal
      Given these guests joined the group pre-order:
        | guest | dishes                 | cart      |
        | Anna  | 2 x dish A             | submitted |
        | Boris | 1 x dish A, 1 x dish B | submitted |
      When the owner opens the group pre-order
      Then the owner sees every guest with their dishes, quantities and subtotal
      And the grand total is the sum of the submitted carts

    @api
    Scenario: The group total is the sum of every participant's total
      Given these guests joined the group pre-order:
        | guest | dishes                 | cart           |
        | Anna  | 2 x dish A             | submitted      |
        | Boris | 1 x dish B, 3 x dish C | still choosing |
      When the owner looks at the participants
      Then the owner sees every guest with their dishes, quantities and subtotal
      And the group total is the sum of every participant's total

    @ui
    Scenario: The owner sees a guest's new cart after the guest changes it
      Given these guests joined the group pre-order:
        | guest | dishes     | cart           |
        | Clara | 1 x dish C | still choosing |
      And the owner opens the group pre-order
      When Clara adds 2 x dish B to her cart
      And the owner refreshes the group pre-order
      Then the owner sees every guest with their dishes, quantities and subtotal

    @api
    Scenario: The group total changes when a guest changes their cart
      Given these guests joined the group pre-order:
        | guest | dishes     | cart           |
        | Anna  | 1 x dish A | submitted      |
        | Clara | 1 x dish C | still choosing |
      And the owner looks at the participants
      When Clara adds 2 x dish B to her cart
      And the owner looks at the participants
      Then the owner sees every guest with their dishes, quantities and subtotal
      And the group total is the sum of every participant's total

  Rule: Guests who have not finished are listed too

    @ui
    Scenario: A guest who is still choosing and a guest without dishes are on the list
      Given these guests joined the group pre-order:
        | guest  | dishes     | cart           |
        | Anna   | 1 x dish A | submitted      |
        | Clara  | 1 x dish C | still choosing |
        | Dmytro |            | still choosing |
      When the owner opens the group pre-order
      Then the owner sees every guest with their dishes, quantities and subtotal
      And the owner is warned that some participants have not submitted their cart

    @api
    Scenario: A guest who has not submitted their cart is listed as still choosing
      Given these guests joined the group pre-order:
        | guest | dishes     | cart           |
        | Clara | 1 x dish C | still choosing |
      When the owner looks at the participants
      Then Clara is listed as still choosing

    @api
    Scenario: A guest who has not added any dish is listed with an empty cart
      Given guest Dmytro joined the group pre-order
      When the owner looks at the participants
      Then Dmytro is listed with no dishes and a total of zero

  Rule: A group nobody has joined yet is empty

    @api
    Scenario: Nobody has joined the group yet
      When the owner looks at the participants
      Then nobody is listed and the group total is zero

    @ui
    Scenario: The owner opens a group nobody has joined yet
      When the owner opens the group pre-order
      Then the owner is told that nobody has added any dishes yet
      And no guest is listed

  Rule: Only the owner of the group sees its participants

    @api @negative
    Scenario Outline: Someone who is not the owner cannot see the participants
      Given guest Anna joined the group pre-order
      When <someone> asks for the participants of the owner's group
      Then <outcome>

      Examples: Signed in, but not the owner
        | someone          | outcome                                    |
        | another customer | they are refused as they are not the owner |
        | a waiter         | they are refused as they are not the owner |

      Examples: Not signed in
        | someone              | outcome                   |
        | a guest of the group | they are asked to sign in |
        | a visitor            | they are asked to sign in |

    @ui @negative
    Scenario: Another customer cannot open the owner's group pre-order
      Given guest Anna joined the group pre-order
      And another customer is signed in on the storefront
      When they open the owner's group pre-order
      Then the group pre-order cannot be loaded
      And no guest is listed

  Rule: A group that does not exist cannot be viewed

    @api @negative
    Scenario: The owner asks for a group that does not exist
      When the owner asks for the participants of a group that does not exist
      Then the owner is told that the group reservation was not found

    @ui @negative
    Scenario: The owner opens the group pre-order of a reservation that does not exist
      When the owner opens the group pre-order of a reservation that does not exist
      Then the owner is told on the screen that the group reservation was not found
