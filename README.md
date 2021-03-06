# NextRepository

Implementing a repository pattern using raw ADO.NET. The implementation is quite simple and suitable for people who want to manipulate database with raw connection, command, transaction and readers.

Currently supports MS Sql & MySql server, but in future, implementation for other sql engines will be added.

A test project is provided to demonstrate the usage of the repository. The repository is thread safe, meaning that you can provide a reference of the same repository to multiple concurrent tasks.

The query, non-query and bulk insert methods uses separate SqlCommands, connection & transaction that would avoid throwing exceptions like 'There is already an open DataReader associated with this Command...'

The Non-Query & BulkInsert provides support to have pre and post operations with in same transaction. The unit tests demonstrates this as well.

# Cache Support

Cache support is added to MS Sql & MySql Repositories. 

It means if the useCache flag is set in the repository, then all the select queries will be cached. 
The caches will be dropped/invalidated as soon as the repository detects an insert/update/delete operation against a similar table for which caches has been generated.

The cache support is demonstrated using Unit Tests, and also through the web sample that is provided along with the solution.

Your feedback is highly appreciated. Please write to me @@ me.shahidali@hotmail.com
