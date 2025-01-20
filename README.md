# RA-BCS

This is RA-BCS (Remote Api-Based Control Server). A "student project" aimed at using API's, listening to incoming messages and launch utilities.    
"Utilities" in this case - YT-DLP. You can find this project at: https://github.com/yt-dlp/yt-dlp    

## About

Objective: Launch YT-DLP at specified path with predefined options and URL that was provided throught API messages. - :white_check_mark:    
Additional objectives: List files from specified folder, and move them to another folder - :white_check_mark:    

## Building

Tools:
1. Visual Studio 2022
2. .NET 9
3. Telegram.Bot library (22.2.0 and above)

____

### Starting up

Directory containing server.exe must have "config.json" file.    
If it doesn't exist - config.json will be created in the directory (if no config.json was found)    
Path variables should have '\' symbols escaped properly - '\\'.    

> WARNING! DO NOT SHARE BOT TOKEN ANYWHERE!