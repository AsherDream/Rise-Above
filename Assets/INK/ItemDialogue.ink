VAR item = ""  // <-- THIS IS THE LINE YOU MUST ADD

== function GetItemDialogue(itemName) ==
{
    - itemName == "BadItem":
        "You clicked the bad item! That hurts."
    - itemName == "GoodItem":
        "You clicked the good item! That feels better."
    - else:
        "You clicked... something."
}

-> END