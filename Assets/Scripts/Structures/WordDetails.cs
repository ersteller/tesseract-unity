using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct WordDetails
{
    public string word;
    public int confidence;
    public Box box;
}