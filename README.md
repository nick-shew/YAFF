# YAFF
"Yet Another FFmpeg Frontend" is an ASP.NET Core MVC frontend for FFmpeg, specifically for audio conversion.

**NOTE:** For legal reasons, you should provide your own compiled version of FFmpeg (available freely at https://ffmpeg.org/). Place it in the /YAFF/ directory and name it `ffmpeg.exe`.

## Limitations
* This currently works only for audio files, with no parameter tweaking supported (yet).
* Max file size is 28.6MB because of IIS.
* Uses default styling 😔
