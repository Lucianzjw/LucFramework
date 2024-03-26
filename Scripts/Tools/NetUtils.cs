namespace JZzzzzzzTools
{
    /*
    using LiteNetLib.Utils;

    public static class NetUtils
    {
        private static NetDataWriter _writer;
        private static NetPacketProcessor _packetProcessor;
        /// <summary>
        /// 序列化写入数组
        /// </summary>
        /// <param name="type"></param>
        /// <param name="packet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static NetDataWriter WriteSerializable<T>(PacketType type, T packet) where T : INetSerializable
        {
            _writer ??= new NetDataWriter();
            _writer.Reset();
            _writer.Put((byte) type);
            packet.Serialize(_writer);
            return _writer;
        }
    
        /// <summary>
        /// 写入包
        /// </summary>
        /// <param name="packet"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static NetDataWriter WritePacket<T>(T packet) where T : class, new()
        {
            _writer ??= new NetDataWriter();
            _packetProcessor ??= new NetPacketProcessor();
            _writer.Reset();
            _writer.Put((byte)PacketType.Serialized);
            _packetProcessor.Write(_writer, packet);
            return _writer;
        }
    
        /// <summary>
        /// 获取当前系统时间的年月日按照"年/月/日"的格式返回
        /// </summary>
        public static string GetTimeNow()
        {
            var time = System.DateTime.Now;
            var year = time.Year;
            var month = time.Month;
            var day = time.Day;
            return year + "/" + month + "/" + day;
        }
    }*/
}