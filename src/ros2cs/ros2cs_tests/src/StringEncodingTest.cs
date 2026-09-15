// SPDX-License-Identifier: Apache-2.0
using NUnit.Framework;

namespace ROS2.Test
{
    public class StringEncodingTest
    {
        private const string Text = "Lyrical 汉字 café 🌍";

        [Test]
        public void Utf8StringSurvivesNativeRoundTrip()
        {
            using var message = new test_msgs.msg.Strings();
            message.String_value = Text;
            message.WriteNativeMessage();
            message.String_value = "overwritten";
            message.ReadNativeMessage();
            Assert.That(message.String_value, Is.EqualTo(Text));
        }

        [Test]
        public void Utf16ScalarArrayAndSequencesSurviveNativeReuse()
        {
            using var message = new test_msgs.msg.WStrings();
            foreach (int size in new[] { 3, 0, 1, 3 })
            {
                message.Wstring_value = Text;
                for (int i = 0; i < message.Array_of_wstrings.Length; i++)
                    message.Array_of_wstrings[i] = Text;
                message.Bounded_sequence_of_wstrings = new string[size];
                message.Unbounded_sequence_of_wstrings = new string[size];
                for (int i = 0; i < size; i++)
                {
                    message.Bounded_sequence_of_wstrings[i] = Text;
                    message.Unbounded_sequence_of_wstrings[i] = Text;
                }
                message.WriteNativeMessage();
                message.Wstring_value = "overwritten";
                message.ReadNativeMessage();
                Assert.That(message.Wstring_value, Is.EqualTo(Text));
                Assert.That(message.Array_of_wstrings, Is.All.EqualTo(Text));
                Assert.That(message.Bounded_sequence_of_wstrings.Length, Is.EqualTo(size));
                Assert.That(message.Unbounded_sequence_of_wstrings.Length, Is.EqualTo(size));
                Assert.That(message.Bounded_sequence_of_wstrings, Is.All.EqualTo(Text));
                Assert.That(message.Unbounded_sequence_of_wstrings, Is.All.EqualTo(Text));
            }
        }
    }
}
