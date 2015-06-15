function abc()
{
    document.write("Button pressed <br />");
    alert("Yo!!");
}
document.write("External JS #Yolo");
alert("This works!!");
document.write("<button id=\"abc\" onclick=\"abc();\">Help me, I'm a button!</button>");
abc();