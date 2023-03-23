INCLUDE GlobalVariables.ink
//similar to a class inheriting from another class

{color== "": -> Color | ->AlreadyChoose}
//similar to the terniary operator in c# (statement)? true: fa


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
        ->Choosen("Blue")
        
    + [<color="red">Red]
        ->Choosen("Red")
        
    + [<color="green">Green]
        ->Choosen("Green")
        
    + [<color="yellow>Yellow]
        ->Choosen("Yellow")
        
=== Choosen(ColorName) ===
//knot's can accpet arguments, similar to a method
Your favorite color is {ColorName}!
-> END 


=== AlreadyChoose ===
You already choose {color}!
-> END


