> **⚠️ WARNING!**
> 
> This project is still in beta and may have some issues. If you experience an issue, please report it and wait until we fix it.


> **⚠️ ANOTHER WARNING**
>
> Since this can use so much of your disk and CPU, it is reccomended to have a mid to high-end setup to use this tool. It is reccomended to have at least a Quad (4) core CPU and a SSD (Solid-State Drive)
> Using this on a HDD may cause this tool to be very slow. It is not reccomended to use a HDD but if you need too, you can but it may be slow.
> The optimal specs would be: >6 core CPU, and an Internal M.2 SSD @ 1GB/s+.

# QuickMD5
A fast, free, small, and open-source MD5 hash checker similar to QuickSFV but console-based.

# How is it so fast
QuickMD5 is very fast in speed because it uses multi-threading. By running multiple threads, it can read and process multiple files at the same time. This greatly reduces the time needed to check the files.

# How to use
To use QuickMD5, you need to have a .MD5 file ready to use in a specific format. Failing to format it proerly may result in errors..

Arguments:
```
QuickMD5.exe PATH_TO_MD5_FILE
```

This is the proper .MD5 format. QuickMD5 will only read files made this way:
```
MD5_HASH *FILEPATH
```
for example:
```
e91c0d2c0c3c0eb39d130443830e1fbd *..\..\bin\x64\Cyberpunk2077.exe
```

Since you can use arguments and do it through a command line, you can set up .bat files to automatically run QuickMD5 and validate files. For example:
```
start path/to/QuickMD5.exe path/to/md5_file.md5
```

# Suggestions
If you would like to suggest a feature, make an issue like this:

Title: [Feature Request] FEATURE NAME

Description: Description of the feature. Explain in detail please.

Picture (optional): If you would like to provide a picture, you may do so.

# Pictures
![QuickMD5 Picture 1](https://github.com/FortNbreak/gg/blob/main/image.png?raw=true)

# Other Information
I have plans on porting this over to python. And no, I do not plan on porting this over to linux. I will do that later once some major updates are out because it is kinda buggy as-is.

