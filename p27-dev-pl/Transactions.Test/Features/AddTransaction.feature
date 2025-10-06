Feature: AddTransaction

A short summary of the feature

@tag1
Scenario: Do not add a transaction with a value less than 0
	Given a value -1
	When add transaction
	Then message should be "Value mus have grather than 0"

Scenario: Do not add a transaction with a value equal to 0
	Given a value 0
	When add transaction
	Then message should be "Value mus have grather than 0"

Scenario: Add a transaction with description Test, value 10, and credit type
	Given a "description" and value 10 and type 0
	When add transaction
	Then message should be ""
