INCLUDE GlobalVariables.ink
//similar to a class inheriting from another class

->Color


=== Color ===
What is your favorite color?
    + [<color="blue>Blue]
        ~color= "BLUE"
        
    + [<color="red">Red]
        ~color= "RED"
        
    + [<color="green">Green]
        ~color= "Green"
        
    + [<color="yellow>Yellow]
        ~color= "YELLOW"
        
- Your favorite color is {color}

Don't forget your favorite color is {color} #TRIGGER_QUEST:FavoriteColor

What is your favorite color?
    + [<color="blue>Blue]
        ~color= "BLUE"
        
    + [<color="red">Red]
        ~color= "RED"
        
    + [<color="green">Green]
        ~color= "Green"
        
    + [<color="yellow>Yellow]
        ~color= "YELLOW"
        
-Don't forget your favorite color is {color}
-> END


