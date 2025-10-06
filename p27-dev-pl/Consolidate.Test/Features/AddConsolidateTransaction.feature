Feature: AddConsolidateTransaction

A short summary of the feature

@tag1
Scenario: Add a new transaction
	Given a "description" and value "10" and date "10/10/2023" and type of "Credit"
	When AddConsolidateTransaction
	Then  message should be ""

#Scenario: Not add a new transaction
#	Given a "description" and value "10" and date "10/10/1989" and type of "Credit"
#	When AddConsolidateTransaction
#	Then  message should be "The date must be valid"
