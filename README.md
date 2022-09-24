# WebAPI-ChatHub
HTTP Server and ChatHub with MySQL database features for https://github.com/Sound932/Chit-Chat.git


# Contains usages of:
* 2 Projects
     * Class Library for managing and executing SQL queries against a MySQL database.
     * Models class library for managing/receiving information from client-side and sending it back out.
* Imgur API to upload Images
     
# Third-Party Libraries:
* Dapper

# Important:
**For anyone trying to use this with the chat project; This project won't run without the correct things set up:**



1 -  **Connection string**: You need to head to DBCommands class and put your connection string in there.

2 - **ChatController class**: You have to provide a client-id from Imgur's api for the functionality of profile pictures/ sending images to work. Head to https://api.imgur.com/ and register an application to receive your client-id. 

Once that's done, head to https://github.com/Sound932/WebAPI-ChatHub/blob/283e8cebc25549e83acd84c6ecc1bb1f097ea4cf/ChattingHub/Controllers/ChatController.cs#L99 and type your client-id in the empty parenthesis.

3 - **EmailService**: Create a random gmail account, then go to https://myaccount.google.com/security and enable 2 step verification. Once you've done that, go to 'app passwords' and generate an application password for the mail app to use on a windows computer. It will display you new app password on a yellow box; use it as your password for the service to function properly.

In https://github.com/Sound932/WebAPI-ChatHub/blob/283e8cebc25549e83acd84c6ecc1bb1f097ea4cf/ChattingHub/Services/EmailService.cs#L22 You type your email that you've created and the password that you got from the yellow box.

And in https://github.com/Sound932/WebAPI-ChatHub/blob/283e8cebc25549e83acd84c6ecc1bb1f097ea4cf/ChattingHub/Services/EmailService.cs#L30 You also type your email.



# Database structure:
https://www.mediafire.com/file/wuqnlu5ifweaxwz/DBStructure.rar/file
