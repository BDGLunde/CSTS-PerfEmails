# CSTS-PerfEmails

Janky/hacked-together version of code (I was unable to get ahold of the finished version) I used to build and e-mail 
(through Office.Interop.Outlook) performance ticket reports to co-workers. 

This was very much a learning experience in regards to working with databases and SQL in addition to creating a tool that saved 
me some time. As a result, some of the code is pretty messy and embarassing - especially the rushed HTMLWriter commands that were
used to build the report. 

Could/should have I simply used SSRS reports for something like this? Yes, but I wasn't very familiar with them at the time and again, this also served as an exercise for learning to use SQL with C#.

The original DBConnection strings and some table names were changed from what is seen in this commit. 

Since I no longer work for Epic Systems, I don't anticipate ever coming back to this project even though it could probably use
some touching up since I first finished it (about a year ago).

