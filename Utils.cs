using System;

namespace DotNet.GitHubAction;
// class for getting action inputs 
public static class Utils
{
 public static string? RemoveLineBreaks(this string input)
 {
    if(input is null)
      return input;

 return input.Replace("\n"," ").Replace("\r"," "); 
 }
}