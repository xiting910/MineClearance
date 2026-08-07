using MineClearance.Core.Models.Records;
using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MineClearance.Infrastructure.Services;

// GameDataRepository 类的 Json 转换器实现部分
internal partial class GameDataRepository
{
    /// <summary>
    /// <see cref="BitArray"/> 的 Json 转换器, 序列化为对象形式, 保留原始位数
    /// </summary>
    private sealed class BitArrayConverter : JsonConverter<BitArray>
    {
        /// <summary>
        /// byte 的位数
        /// </summary>
        private const int BitsPerByte = 8;

        /// <summary>
        /// 保存 <see cref="BitArray"/> 的字节数组属性名
        /// </summary>
        private const string BytesPropertyName = "Bytes";

        /// <inheritdoc/>
        public override BitArray? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;
            return new(root.GetProperty(BytesPropertyName).GetBytesFromBase64())
            {
                Length = root.GetProperty(nameof(BitArray.Length)).GetInt32()
            };
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, BitArray value, JsonSerializerOptions options)
        {
            var bytes = new byte[(value.Length + BitsPerByte - 1) / BitsPerByte];
            value.CopyTo(bytes, 0);
            writer.WriteStartObject();
            writer.WriteNumber(nameof(BitArray.Length), value.Length);
            writer.WriteBase64String(BytesPropertyName, bytes);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// <see cref="Position"/> 的 Json 转换器, 值序列化为对象, 字典键序列化为字符串
    /// </summary>
    private sealed class PositionConverter : JsonConverter<Position>
    {
        /// <summary>
        /// 作为属性时的分割字符
        /// </summary>
        private const char PropertyNameSeparator = ',';

        /// <inheritdoc/>
        public override Position Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;
            return new Position(
                root.GetProperty(nameof(Position.Row)).GetInt32(),
                root.GetProperty(nameof(Position.Col)).GetInt32()
            );
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Position value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(Position.Row), value.Row);
            writer.WriteNumber(nameof(Position.Col), value.Col);
            writer.WriteEndObject();
        }

        /// <inheritdoc/>
        public override Position ReadAsPropertyName(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var key = reader.GetString() ?? throw new JsonException("The property name is null.");
            var parts = key.Split(PropertyNameSeparator);
            return new(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        /// <inheritdoc/>
        public override void WriteAsPropertyName(
            Utf8JsonWriter writer,
            Position value,
            JsonSerializerOptions options)
        {
            writer.WritePropertyName($"{value.Row}{PropertyNameSeparator}{value.Col}");
        }
    }
}
