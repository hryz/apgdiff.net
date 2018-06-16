# PostgreSQL schema compare tool

This is a .Net port of the v 2.4 of the _"Another PostgreSQL Diff tool (apgdiff)"_  
[https://github.com/fordfrog/apgdiff](https://github.com/fordfrog/apgdiff)  

The main reason for this port is to add UI to the original tool.  
Everyone who likes the SQL Server schema compare tool (aka database projects in Visual Studio) wants to have the same kind of experience with PostgreSQL. The original tool can be used in the automation scripts, but it lacks UI.  
The UI will contain 3 panels just like any merge tool: source, destination, diff.  
Also, full control over the code allows altering the behavior. For example, support for storing the tables in individual files can be added (mimicking the SQL Server database project).  

## WARNING
The code can contain issues, the performance might not be ideal. It is a quick port, the code quality can be improved dramatically.