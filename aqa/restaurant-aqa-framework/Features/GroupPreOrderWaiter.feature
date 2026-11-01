@group-preorder
Feature: The waiter receives the group pre-order

  What the owner submits reaches the restaurant: the waiter who serves the reserved
  table sees the dishes of the submitted carts and of the owner's own pre-order, and
  nothing from guests who were removed or never submitted their cart.

  Background:
    Given the owner has booked a table served by the waiter of the location for a group of 5
    And the owner has pre-ordered 1 x dish D for themselves

  Rule: The waiter sees what the owner submitted

    @ui @L2
    Scenario: The waiter of the location sees the submitted pre-order
      Given these guests joined the group pre-order:
        | guest | dishes                 | cart           |
        | Anna  | 2 x dish A             | submitted      |
        | Boris | 1 x dish A, 1 x dish B | submitted      |
        | Clara | 1 x dish C             | still choosing |
      And the owner has submitted the group pre-order
      When the waiter of the location opens the pre-order of the reservation
      Then the waiter sees these dishes:
        | dish   | quantity |
        | dish A | 3        |
        | dish B | 1        |
        | dish D | 1        |

  Rule: The whole journey, from the guests' carts to the waiter

    @ui @L3 @e2e
    Scenario: A removed guest's dishes never reach the waiter
      Given these guests joined the group pre-order:
        | guest | dishes                 | cart      |
        | Anna  | 2 x dish A             | submitted |
        | Boris | 1 x dish B, 1 x dish C | submitted |
        | Clara | 1 x dish A             | submitted |
      And the owner opens the group pre-order
      And the owner sees every guest with their dishes, quantities and subtotal
      When the owner removes Boris from the group pre-order
      And the owner submits the group pre-order
      Then the owner is told that the pre-order was sent to the restaurant
      And the consolidated pre-order is:
        | dish   | quantity |
        | dish A | 3        |
      When the waiter of the location opens the pre-order of the reservation
      Then the waiter sees these dishes:
        | dish   | quantity |
        | dish A | 3        |
        | dish D | 1        |
