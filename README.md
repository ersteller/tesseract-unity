# About This Project
This project aims to solve the following problem. <br>
Imagine there is a document and you want to find a specific word. On a webpage you would enter search mode and type the word. All Instances of the word would be instantly be highlighted four you. In real life documents this was not possible until now.

# Standalone OCR for Unity using Tesseract 
This is a sample project that sets up API and Dependencies to use Tesseract for different platforms.
To read more about how this project was made visit [this Article](https://medium.com/@neelarghyamandal/offline-ocr-using-tesseract-in-unity-part-1-b9a717ac7bcb) 

## Requires
Unity

## Usage
This demo shows frames from the camera parsed in realtime.
Enter a word in the textfield that is searched and if found marked in the original image.

## Limitations / issues
```
UI/UX: Usability needs to be improved.
      Search Text is relay small after entering. 
      Found box borders are realy slim especially in UHD. (wider border or even pointy sprite -->[arrow])
Orientation: Text is not correctly recognized when tilted
      Getting gravity vector and rotate input image according would limit the text to be only horizontally. Maybe add guide line for user instead.
      Portrait- and landscape mode need to be fixed.
Memory: 
      There are leaks.
Optimization: 
      The routines are poorly written and contain superflous copy and function calls. 
      Maybe there is something to preprocess / select in the texture or format to improve recognition time.
      Improve wording of documentation
```

## Libs
tesseract cross platform Libs were build with [rhardih/bad](https://github.com/rhardih/bad)
and libc++_shared.so copied from https://github.com/urho3d/android-ndk.git

## License
```
The code in this repository is licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```