@AndrePires
Feature: Engine.For Behaviors
	Some new behaviors for the Engine type

@AndrePires
Scenario: Engine For should work correctly when using generic models
Whe we use generic models with the Engine it is not being able
to execute commands and queries correctly. That is because is should use _ instead of `1 when 
creating the folders for Journal and Snapshot files when using generic model.
	Given a generic model
	And an Engine for that model
	And a command
	And a query
	When executing the command using that engine
	Then the command should work
	When executing the query using that engine
	Then the query should work
