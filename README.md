###Contact Manager

**ContactManager** was born out of a frustration with Outlook and Gmail contact management, which don't include the notion of 'household' - this makes it difficult to manage (for example) a family's address, at the same time as individual mobile phone numbers.

**ContactManager** is very basic: it doesn't send or receive emails, doesn't do mail merges. All it does is manage contacts, and people. A contact can be a business, or a family. Each contact can 'contain' zero or more people. There are a limited number of fields that ContactManager knows about (address, phone number, email, birthday etc.). Anything else can be added to the 'Notes' field. 

*For more info about **using** ContactManager* install it and checkout Help, or browse the source code for `Data/HelpDescription.html`. Everything that follows is aimed at developers.

###History

This is a homegrown project for my own use. As such, I ran out of steam when it got 'good enough' for my needs. There is still a lot which could be done, but currently, the effort required to improve the functionality is greater than the effort required to put up with the various niggles of usage. Putting the code up here on GitHub is partly an attempt to re-engage with the code.

###Code

- C#/XAML using Visual Studio
- I've replaced the built-in VS tests with [XUnit](http://xunit.codeplex.com/) tests, which means that even people with [VSExpress](http://www.microsoft.com/express/Downloads/#2010-Visual-CS) should be able to compile and contribute.
- The tests have pitiful coverage, as I've no idea how to do unit tests of all the UI stuff.
- The code is also 'good enough', lots of places could benefit from some refactoring.

####Quick tour
- I've used ` System.Xml.Serialization` for serialization, it works ok but requires some fiddling/compromises (notably for saving a nullable DateOfBirth)
- By convention, Contacts are immutable when they are in the AddressBook - any modifications are performed on a copy, which is then swapped. (This makes the code easier to reason about, and also makes Undo/Redo possible)
- The 'full text search' currently used in the filter just works by concatenating all the strings in a contact together then searching within that. Nothing clever.
- Undo/Redo works with two stacks of UndoRedoItems, which just have a description and a list of before/after contacts. Surprisingly simple.
- A few properties are saved in user settings (see `app.config`)
- Help works by writing an html file to disk then loading it in a browser control. Seemed like a good idea at the time.
- Liberal use of databinding throughout (the project actually started as WinForms, and databinding was the *only* reason for switching to WPF)

####Dead zones 
There are some 'dead zones' in the code 

- `Data/reduce.xslt` generates much smaller xml data files by chopping down to one-letter elements and attributes. Currently not used (zipping would probably be the best way to go if this was really a concern)
- The `AddressBook.xslt` and `AddressBook.css` files are not dead as such, I created them for read-only access to Address book data files when the ContactManager program is not available. Works in Chrome and Firefox, not so much in versions of IE I've tested.

###Roadmap
There is no roadmap as such, but the current *philosophy* is to keep **ContactManager** as simple as possible. I'd rather polish what's already there than add new functionality. 

That said, here are a list of things that I have in mind:


**Bugs**

- Null exceptions on some buttons for a new (empty) address book.

**Usability**

- For reasons I can no longer recall, the ContactDetail form is an unmovable modal dialog, this means that if you move ContactManager too low on the screen, you can no longer get at the Save and Cancel buttons.
- Tool tips
- Add keyboard shortcuts (undo/redo/save etc.)
- Generally improve 'lookologie'
- Vista/Windows7-style Yes/No/Cancel dialog for 'Unsaved changes' dialog
- Currently hitting any letter will scroll to contacts starting with that letter. It would be good to introduce a timer so that a second letter typed within x milliseconds scrolls to contacts starting with those two letters.

**Functionality**

- Permit localised date formats
- Permit comma-separated csv (currently only semi-colon separated, because it's in my Regional Settings, and it's easier)
- Introduce notion of 'archive/hide' (instead of delete)
- Categories? Currently you can get pseudo-categories using the Notes field and filtering on that. But that forces you to retype the category text each time. Instead you could have 2 or 3 'category' fields with combo/text where any already-entered texts would be listed in the combo. This would be especially useful (to me) for reducing the list of birthdays I want to be alerted about.
- One thing I'd really like, but I haven't worked out how to do, is to render the window partially transparent when you press Ctrl (like VS intellisense popups) - this would be really useful when transcribing addresses.
- Right click & copy on email addresses

**Code**

- Localise texts (for other languages)
- MVC
- Better/more tests
- Implement drag/drop of People
- Pay more attention to scope - too much public stuff currently.