using System;

public class MessageBusMethodAttribute : Attribute
{
    public string Description
    {
        get;
        private set;
    }

    public MessageBusMethodAttribute(string description = "")
    {
        Description = description;
    }
}
