@group-preorder @L2
Feature: Submitting the group pre-order

  After reviewing the guests' carts the owner submits one consolidated pre-order. Only
  the carts the guests submitted are taken, the same dishes from different guests are
  merged into one line with the total quantity, the dishes are added to the owner's own
  pre-order, and the restaurant receives it.

  Background:
    Given the owner has booked a table for a group of 5

  Rule: Only submitted carts are sent, and the same dishes are merged

    @ui @smoke
    Scenario: The owner submits the consolidated pre-order
      Given the owner has pre-ordered 1 x dish D for themselves
      And these guests joined the group pre-order:
        | guest | dishes                 | cart           |
        | Anna  | 2 x dish A             | submitted      |
        | Boris | 1 x dish A, 1 x dish B | submitted      |
        | Clara | 1 x dish C             | still choosing |
      And the owner opens the group pre-order
      When the owner submits the group pre-order
      Then the owner is told that the pre-order was sent to the restaurant
      And the consolidated pre-order is:
        | dish   | quantity |
        | dish A | 3        |
        | dish B | 1        |
      And the restaurant's order holds the owner's dishes and the consolidated pre-order
      And the order is marked as submitted by the customer
      And the group pre-order is marked as submitted

    @api
    Scenario: The service merges the same dishes and leaves out unsubmitted carts
      Given the owner has pre-ordered 1 x dish D for themselves
      And these guests joined the group pre-order:
        | guest | dishes                 | cart           |
        | Anna  | 2 x dish A             | submitted      |
        | Boris | 1 x dish A, 1 x dish B | submitted      |
        | Clara | 1 x dish C             | still choosing |
      When the owner submits the group pre-order
      Then the consolidated pre-order is:
        | dish   | quantity |
        | dish A | 3        |
        | dish B | 1        |
      And the restaurant's order holds the owner's dishes and the consolidated pre-order
      And the order is marked as submitted by the customer
      And the group pre-order is marked as submitted

  Rule: There must be something to send

    @api @negative
    Scenario: Nobody has submitted a cart yet
      Given the owner has pre-ordered 1 x dish D for themselves
      And these guests joined the group pre-order:
        | guest | dishes     | cart           |
        | Clara | 1 x dish C | still choosing |
      When the owner submits the group pre-order
      Then the owner is told "There are no submitted guest sessions in this group reservation."

    @ui @negative
    Scenario: The owner cannot submit while no guest has submitted a cart
      Given these guests joined the group pre-order:
        | guest | dishes     | cart           |
        | Clara | 1 x dish C | still choosing |
      When the owner opens the group pre-order
      Then the owner cannot submit the group pre-order

    @api @negative
    Scenario: The submitted carts have no dishes in them
      Given the owner has pre-ordered 1 x dish D for themselves
      And guest Anna joined the group pre-order
      And Anna submitted her cart and then took every dish out of it
      When the owner submits the group pre-order
      Then the owner is told "The cart is empty"

    @api @negative
    Scenario: The owner has no pre-order of their own for the reservation
      Given these guests joined the group pre-order:
        | guest | dishes     | cart      |
        | Anna  | 2 x dish A | submitted |
      When the owner submits the group pre-order
      Then the owner is told that their own pre-order for the reservation was not found

    @ui @negative
    Scenario: The owner without a pre-order of their own is told why the submit failed
      Given these guests joined the group pre-order:
        | guest | dishes     | cart      |
        | Anna  | 2 x dish A | submitted |
      And the owner opens the group pre-order
      When the owner submits the group pre-order
      Then the owner is told on the screen that their own pre-order for the reservation was not found
      And the group pre-order is still collecting pre-orders

    @api @negative
    Scenario: The owner submits a group that does not exist
      When the owner submits the pre-order of a group that does not exist
      Then the owner is told that the group reservation was not found

  Rule: A group pre-order is submitted only once

    @api @negative
    Scenario: The owner submits the same group pre-order again
      Given the owner has pre-ordered 1 x dish D for themselves
      And these guests joined the group pre-order:
        | guest | dishes     | cart      |
        | Anna  | 2 x dish A | submitted |
      And the owner has submitted the group pre-order
      When the owner submits the group pre-order
      Then the owner is told "Group reservation was submitted."

    @ui
    Scenario: A submitted group pre-order can no longer be changed
      Given the owner has pre-ordered 1 x dish D for themselves
      And these guests joined the group pre-order:
        | guest | dishes     | cart      |
        | Anna  | 2 x dish A | submitted |
      And the owner opens the group pre-order
      When the owner submits the group pre-order
      Then the group pre-order can no longer be changed

  Rule: Only the owner submits the group pre-order

    @api @negative
    Scenario Outline: Someone who is not the owner cannot submit
      Given the owner has pre-ordered 1 x dish D for themselves
      And these guests joined the group pre-order:
        | guest | dishes     | cart      |
        | Anna  | 2 x dish A | submitted |
      When <someone> submits the owner's group pre-order
      Then <outcome>
      And the group pre-order is still open

      Examples: Signed in, but not the owner
        | someone          | outcome                                    |
        | another customer | they are refused as they are not the owner |
        | a waiter         | they are refused as they are not the owner |

      Examples: Not signed in
        | someone   | outcome                   |
        | a visitor | they are asked to sign in |
