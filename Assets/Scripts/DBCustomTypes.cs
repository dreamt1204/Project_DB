using ExitGames.Client.Photon;
using UnityEngine;


/// <summary>
/// Internally used class, containing de/serialization methods for various Unity-specific classes.
/// Adding those to the Photon serialization protocol allows you to send them in events, etc.
/// </summary>
internal static class DBCustomTypes
{
    /// <summary>Register</summary>
    internal static void Register()
    {
        PhotonPeer.RegisterType(typeof(Character), (byte)'C', SerializeCharacter, DeserializeCharacter);
    }


    #region Custom De/Serializer Methods

    #region Memory sizes
    const int INTEGER_BYTE = 1; // Unsigned (0 to 255)
    const int INTEGER_SBYTE = 2; // Signed (-128 to 127)
    const int INTEGER_SHORT = 2; // Signed (-32,768 to 32,767)
    const int INTEGER_USHORT = 2; // Unsigned (0 to 65,535)
    const int INTEGER_INT = 4; // Singed (-2,147,483,648 to 2,147,483,647)
    const int INTEGER_UINT = 4; // Unsigned (0 to 4,294,967,295)
    const int INTEGER_LONG = 8; //  Signed (-9,223,372,036,854,775,808 to 9,223,372,036,854,775,807)
    const int INTEGER_ULONG = 8; // Unsinged (0 to 18,446,744,073,709,551,615)
    const int FLOAT_FLOAT = 4; // ±1.5e−45 to ±3.4e38  (Precision:7 digits)
    const int FLOAT_DOUBLE = 8; // ±5.0e−324 to ±1.7e308 (Precision:15-16 digits)
    const int FLOAT_DECIMAL = 16; // (-7.9 x 1028 to 7.9 x 1028) / (100 to 28) (Precision:28-29 digits)
    const int CHARACTER_CHAR = 2;
    const int OTHER_DATETIME = 8;
    const int OTHER_BOOL = 1;
    #endregion

    public static readonly byte[] memCharacter = new byte[INTEGER_INT];
    static short SerializeCharacter(StreamBuffer outStream, object customObject)
    {
        int ViewID = ((Character)customObject).gameObject.GetPhotonView().viewID;

        lock (memCharacter)
        {
            int index = 0;
            byte[] bytes = memCharacter;
            Protocol.Serialize(ViewID, bytes, ref index);
            outStream.Write(bytes, 0, memCharacter.Length);
        }

        return (short)memCharacter.Length;
    }

    static object DeserializeCharacter(StreamBuffer inStream, short length)
    {
        int ViewID;

        lock (memCharacter)
        {
            inStream.Read(memCharacter, 0, length);
            int index = 0;
            Protocol.Deserialize(out ViewID, memCharacter, ref index);
        }

        if (PhotonView.Find(ViewID) == null)
            return null;

        return PhotonView.Find(ViewID).gameObject.GetComponent<Character>();
    }

    #endregion
}

