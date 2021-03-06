# FAQ

These are real questions, that I have been asked.

#### _Why should I use Pathfinder?_

It will help you find your way to a good Sitecore implementation.

The motto of Pathfinder is "Get started, get far, get happy".

* Get started: Pathfinder helps you get started with Sitecore and your project in the right way.
* Get far: Pathfinder helps you with a lot of tasks.
* Get happy: When you finally encounters something, Pathfinder does not help you with, it is so 
  easy to extend Pathfinder, that you get happy.

Using Pathfinder is not mandatory, but hopefully Pathfinder will make your life a bit easier 
when developing Sitecore websites.  

If you already have a successful development setup, you can use the ideas in Pathfinder as
inspiration.

#### _Who should use Pathfinder?_

Developers. 

Pathfinder is aimed at the development process.

#### _I don't want to edit items as files - I want Content Editor, Experience Editor Sitecore Rocks, or DB Browser?_

You can use TDS or Unicorn to serialize changed items back to the Pathfinder project. Pathfinder supports both 
.item (TDS) and .yml (Unicorn) formats. 

You configure TDS or Unicorn to serialize the items to your Pathfinder project folder, and whenever you make 
a change on the website, make sure you serialize it, before compiling with Pathfinder. 

If you do not use TDS or Unicorn, you can serialize item using either the Sitecore Rocks serialization commands
or use the Developer ribbon tab in the Content Editor. Make sure you config the serialization folder correctly
in the web.config.

#### _Xml is so old school / you have to type so much in Xml_

Well, use Json or Yaml instead.

#### _Json is so hard to read / json is a terrible serialization format_

Well, use Xml or Yaml instead.

#### _Pathfinder does not scale to big projects_

This is possibly true - Pathfinder has not yet been use in a large project.

However there is nothing in the architecture of Pathfinder, that prevents it from scaling. 

It is a compiler that processes a number of input files and produces an output file.

In a project with 5.000 items, Pathfinder runs like this:

```cmd
Task 'restore-packages': 4ms
scc.cmd(0,0): information SCC3041: Checking project...
scc.cmd(0,0): information SCC3042: Checks: 44, errors: 0, warnings: 0, messages: 0
Task 'check-project': 2.958ms
scc.cmd(0,0): information SCC4015: Writing package exports...
Task 'write-exports': 20ms
scc.cmd(0,0): information SCC4018: Creating packages...
scc.cmd(0,0): information SCC4019: Created: sitecore.project\Sitecore.nupkg (2.837.414 bytes)
Task 'pack-nuget': 25.753ms
scc.cmd(0,0): information SCC4008: Installing packages...
scc.cmd(0,0): information SCC4009: Installed: Sitecore.nupkg
Task 'install-package': 31.140ms
scc.cmd(0,0): information SCC4016: Publishing database...
scc.cmd(0,0): information SCC4014: Database: master
Task 'publish-database': 65ms
Project metrics: 5057 items, 0 templates, 0 media files, 0 renderings, 0 files
Time: 60.233ms, Ducats earned: 0
Task 'show-status': 5ms
```

Overall the performance is acceptable.

#### _Can I use just the checking parts of Pathfinder and not all the other stuff?_

Sure - just run the `check-project` task and none of the other tasks.

#### _Can I use Pathfinder as part of CI?_

Sure - Pathfinder is a command-line tool, so it will run on all Windows installations - including
in the cloud.

We have used in a Visual Studio Online project.

#### _Does Pathfinder replace TDS or Unicorn?_

No.

But since items are files, serialization of items is implicit.

Pathfinder should work well with TDS and Unicorn, if you want to use the Content Editor, Experience Editor,
Sitecore Rocks or DB Browser to edit items.
