# EmailPerformanceReport
Retrieves information from the NGP 7 API to create a CSV file report about emails sent out.
Uses RestSharp, C# and .Net 6.0

## Setup and Running
First compile the code (or use the files provided in the Release folder). Visual Studio 2022 with RestSharp installed from Nuget Package Manager is needed to compile.

Open command line and change to the directory holding the compiled files. 
Run EmailPerformanceReport.exe with the following command:
```
EmailPerformanceReport.exe <API username> <API password> [file name]
```
File name will be prepended to be under the current directory and appended with ".csv". By default this will create the file EmailReport.csv.

Command line arguments can be provided in the launch profile to easily test test.

## Follow Up Questions
> How long, roughly, did you spend working on this project?

About 4 hours. I had some trouble connecting to the API initially.

> What could you do to improve your code for this project if you had more time? Could you make it more efficient, easier to read, more
maintainable? If it were your job to run this report monthly using your code, could it be made easier or more flexible?

I would improve the error handling, especially to be more robust about when the API does not respond or bad parameters were given. Best error handling practices depend 
greatly on where and how the code is being used though.

Efficiency could be improved greatly if the API could be extended or changed to avoid having to requery so much to get more information about
records already queried for. I can imagine there are many scenarios where changing the API would be out of the question, but it would be the most impact change to
performance.

If there proved to be a lot of records I would look into making the section where it requeries for information about statistics and variants
to be more asynchronous or threaded. The current way it is written makes it hard to use those as they all would want to add data to a list, which isnt thread safe.
This could be reworked, but in general this would be harder to maintain and read, and it also carries the risk of kicking off too many calls to the API at one time (which can overload the API or cause it to reject the requests).

The sorting was done via LINQ which is not incredibly transparent about its effiency. It might be better to move to using a specific data structure 
(rather than IEnumerable) for sorting (and determining the top variant) to be able to do the sorting in place and have more control over the sorting algorithm.
The time complexity would not likely improve much, though the space efficiency would be greatly improved by sorting in place. 

Using a datastructure like a max priority queue could make the sorting not required for the list of emails. However, RestSharp wouldn't be able to bind the response data to a model using this though and so you would still have an additional a time complexity of `O(nlog(n))` to create the priority queue from the list in the response. Other data structures have similar issues and the most maintainable and readable option is to just use something that implements IEnumerable like List.

The code used to merge the data from the different queries could be improved to be more space efficient by mutating the records in the list created by the query rather 
than making a new list. Foreach loops in this case were easier to read and maintain though in my opinon.

If I had to run this often I would want to use some sort of key store or configuration to store the API username and password to avoid having to provide it in parameters
every time. Depending on where this code is being used, this whole program could be made into a service and those often use configurations to safely store and use
that sort of data. This would also likely increase security as it is likely people would just store the API username and password somewhere else to be copied and 
pasted, which is often not secure.

>Outline a testing plan for this report, imagining that you are handing it off to someone else to test. What did you do to test this as you
developed it? What kinds of automated testing could be helpful here?

### A List of Things to Test
- Does the code compile and does it run without error provided the correct parameters?
- Does the code fail gracefully if provided incorrect or missing parameters?
- Does the code create a file with the correct name?
- Is the data in the file in correct format?
- Is the data in the file correct?
- Does the code fail gracefully without internet connection?
- Does the code fail gracefully when provided bad data by the API?

It is hard to test if the top variant is correct as a user since you do not get a list of variants and their statistics for each email. 
A user would need to repeat the same queries to the api the code does internally using other tools and double check the data matches up and that the top 
variant was calculated correctly.

I tested most of these as I went, though I did make the assumption because of time constraints that the API would work correctly and be available. Testing as I 
developed I would use breakpoints to watch data and check the responses of the API to ensure the data was correct and binding to models. Once it was in a more
complete state I started testing bad user inputs and checking if the resulting file was correct.

### Automated Testing
Unit testing could be implemented. Unit testing can be paired with other automated frameworks to run and test the code for deployments or commits and alert 
developers if tests fail (and can also be run manually).
