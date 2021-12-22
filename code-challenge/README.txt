BUG: Database lifetime was scoped, so when changes were saved (specifically, when DirectReports was mapped) they did not still exist later.
Fixed by adding ServiceLifetime.Singleton parameters, to ensure database would exist for the duration of the program.  HOWEVER! db contexts
are generally supposed to be short-lived, not long-lived.  Research suggests this is now not thread-safe (which I suspected), which is bad.
I'd LIKE to use a dbContextFactory, but that doesn't exist until .NET CORE 5.  Which is not a game-ender...this project is using .NET CORE 2.1,
which went EOL in August.  The code base should be updated anyway.  I assume that's outside the scope of this project though.
Additionally, I believe I know how to fix this bug while staying in 2.1, but it would not use DI, so I opted to not go that route.

TLDR: As a developer, I am officially saying that the proper solution to this is NOT setting the lifetime of the dbContext to be Singleton,
but is instead to update the framework entirely so that more tools are available.

TASK 1:
- We are making an assumption that there isn't an accidental loop in the database where a subordinate actually becomes a supervisor of someone above them.
- Additionally, we are making an assumption that in no instance will an employee be a subordinate to more than one person.

TASK 2:
- PER SPEC, we are NOT creating an endpoint that will allow you to modify a Compensation.  CREATE and READ only.
- Spec doesn't say we can't add more than just the 3 properties specified.  Added a 4th property, CompensationId, to bind as the primary key.

TESTS:
- Tests have been added to test both of the new circumstances introduced in this challenge.
- IMPORTANT: Some of the new tests assume a fresh database.  This is only because there was no "delete" compensation required in the spec.  Consequently,
  InitializeClass (and associated attribute) and CleanUpTest (and associated attribute) have been refactored to start a new instance of the API for 
  EVERY SINGLE TEST.
- All tests have passed.  If you experience different behavior, please reach out to me so we can investigate together.


MISC. COMMENTS:
- I don't like EmployeeService.Replace.  If newEmployee is null, then you're performing a delete operation instead of a replace operation.  A
  delete operation should be its own operation.  The replace operation can call the delete operation, but it should still be a discrete operation
- Why are you using the unit of work/service/repository pattern?  EF already does this, more or less, with DbContext and DbSet.  I found this
  while investigating the bug, and agree with it: https://entityframeworkcore.com/knowledge-base/50457007/in-memory-database-doesn-t-save-data.  The
  code becomes much cleaner, in my mind.  I would be interested to have a discussion as to why UoW/service/repo might be a better fit.
- You are using two different patterns to handle async things.  Some places (like the seeder) use true async/await, and most others use async/.Wait().
  An api like this should ideally use async/await to get true async behavior.  Otherwise, you're just running synchronously.
- Update your framework!  If your response is "we use .NET Core 2.1 because that's what our code base uses", then my response is: UPDATE YOUR FRAMEWORK.
