# RA-BCS

This is RA-BCS (Remote Api-Based Control Server). A "student project" aimed at using API's, listening to incoming messages and launch utilities.    
"Utilities" in this case - YT-DLP. You can find this project at: https://github.com/yt-dlp/yt-dlp    

## About

Objective: Launch YT-DLP at specified path with predefined options and URL that was provided throught API messages. - :white_check_mark:    
Additional objectives: List files from specified folder, and move them to another folder - :black_square_button:    

## Building

Tools:
1. Visual Studio 2022
2. .NET 9
3. Telegram.Bot library (22.2.0 and above)

____

### Starting up

Directory containing server.exe must have "secret.txt" and "yt_dlp_path.txt" file.    
It should only have 1 line - bot token path to yt_dlp.exe file on your computer.    
Path should not be in "" symbols.    

> WARNING! DO NOT SHARE BOT TOKEN ANYWHERE!