using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Messages
{
    private static Dictionary<MessageType, (string, int)> _messageDict = new()
    {
        { MessageType.DEFAULT, ("PEACEKEEPING OPERATION UNDERWAY::PLEASE REMAIN CALM", 0) },
        { MessageType.DMG_CONVENTIONAL, ("WARNING::UNAUTHORIZED USE OF FORCE DETECTED", 1) },
        { MessageType.DMG_EXPLOSIVE, ("DANGER::HIGH EXPLOSIVE MUNITIONS DETECTED", 2) },
        { MessageType.PV_DEATH, ("HOSTILE TARGET EXPIRED::THANK YOU FOR YOUR COMPLIANCE", 3) },
        { MessageType.ACS_DEATH, ("UNABLE TO RESOLVE HOST {0}::PLEASE CONTACT AN ADMINISTRATOR", 4) },
    };

    public static MessageData GetMessage(MessageType messageType)
    {
        MessageData messageData = new();

        (string message, int priority) = _messageDict[messageType];

        if (messageType == MessageType.ACS_DEATH)
        {
            int randomInt = Random.Range(0, 9999);
            string randomName = string.Format("ACSPU_{0}@ACS.LIMURA.GOV", randomInt);
            message = string.Format(message, randomName);
        }

        messageData.Message = message;
        messageData.Priority = priority;

        return messageData;
    }
}

public struct MessageData
{
    // Message string displayed
    public string Message;

    // Message is only used is priority is higher than current message
    public int Priority;
}

public enum MessageType { DEFAULT, DMG_CONVENTIONAL, DMG_EXPLOSIVE, PV_DEATH, ACS_DEATH };
