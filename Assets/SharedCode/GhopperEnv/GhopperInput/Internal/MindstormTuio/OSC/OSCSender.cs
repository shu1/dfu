using System;
using System.Net.Sockets;
using OSC;
using System.Text;
using System.IO;

public class OSCSender {
	private string remoteAddress = "127.0.0.1";
    private int remotePort = 3335;
	
	UdpClient udpSender = null;
	bool isRunning = false;
	
	public OSCSender () {
	    //
	}
	
	public void SetAddress(string addr) {
		remoteAddress = addr;
	}
	
	public void SetPort(int portNum) {
		remotePort = portNum;
	}
	
	public void Start()
    {
        if (!isRunning)
        {
			DebugExtras.Log("osc sender opening.");
    		try
	        {
	            udpSender = new UdpClient();
	            isRunning = true;
//	            return true;
	        }
	        catch (Exception e)
	        {
	            DebugExtras.LogWarning("failed to open udp sender");
	            DebugExtras.LogWarning(e);
	        }
	
//	        return false;
        }
    }
	
	public bool IsRunning() {
		return isRunning;
	}
	
	public void Stop()
    {
        if (isRunning)
        {
            isRunning = false;
            close();
        }
    }
	
	void close()
    {
		DebugExtras.Log("osc sender closed.");
        try
        {
            //Might throw an exception which is meaningless when we are shutting down
            udpSender.Close();
        }
        catch
        {

        }
    }
	
	public void Send(OSCMessage message)
	{   
		byte[] packet;// = message.BinaryData; // not reliable!
		int length;// = packet.Length; 
//	    SendPacket(packet, length); // ... occasionally screws up! game/init "game" works, but game/show "game" fails (claiming "game" wasn't included!)
		// when it screwed up, packet was 4 bytes shy! (48 v 52)
		
		packet = new byte[1000];
		length = OscMessageToPacket(message, packet, 0, 1000);
//		DebugExtras.Log("alt length: "+length.ToString());
		SendPacket(packet, length);
        if ( GhopperDaemon.detailedDebug )
		    DebugExtras.Log( message.ToString() );
	}
	
	public void SendPacket(byte[] packet, int length)
	{   
	    udpSender.Send(packet, length, remoteAddress, remotePort);
        if ( GhopperDaemon.detailedDebug )
	        DebugExtras.Log("osc message sent to "+remoteAddress+" port "+remotePort+" len="+length);
	}
	
	// alternate pack method -- need to compare this with OSCMessage's & see where the issue is.
	private static int OscMessageToPacket(OSCMessage oscM, byte[] packet, int start, int length)
    {
      int index = start;
      index = InsertString(oscM.Address, packet, index, length);
      //if (oscM.Values.Count > 0)
      {
        StringBuilder tag = new StringBuilder();
        tag.Append(",");
        int tagIndex = index;
        index += PadSize(2 + oscM.Values.Count);

        foreach (object o in oscM.Values)
        {
          if (o is int)
          {
            int i = (int)o;
            tag.Append("i");
            packet[index++] = (byte)((i >> 24) & 0xFF);
            packet[index++] = (byte)((i >> 16) & 0xFF);
            packet[index++] = (byte)((i >> 8) & 0xFF);
            packet[index++] = (byte)((i) & 0xFF);
          }
          else
          {
            if (o is float)
            {
              float f = (float)o;
              tag.Append("f");
              byte[] buffer = new byte[4];
              MemoryStream ms = new MemoryStream(buffer);
              BinaryWriter bw = new BinaryWriter(ms);
              bw.Write(f);
              packet[index++] = buffer[3];
              packet[index++] = buffer[2];
              packet[index++] = buffer[1];
              packet[index++] = buffer[0];
            }
            else
            {
              if (o is string)
              {
                tag.Append("s");
                index = InsertString(o.ToString(), packet, index, length);
              }
              else
              {
                tag.Append("?");
              }
            }
          }
        }
        InsertString(tag.ToString(), packet, tagIndex, length);
      }
      return index;
    }
	
	private static int InsertString(string s, byte[] packet, int start, int length)
    {
      int index = start;
      foreach (char c in s)
      {
        packet[index++] = (byte)c;
        if (index == length)
          return index;
      }
      packet[index++] = 0;
      int pad = (s.Length+1) % 4;
      if (pad != 0)
      {
        pad = 4 - pad;
        while (pad-- > 0)
          packet[index++] = 0;
      }
      return index;
    }
	
	private static int PadSize(int rawSize)
    {
      int pad = rawSize % 4;
      if (pad == 0)
        return rawSize;
      else
        return rawSize + (4 - pad);
    }
}