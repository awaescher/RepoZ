namespace Tests.Common
{
    using NUnit.Framework;
    using System;
    using FluentAssertions;
    using RepoZ.Api.Common;

    public class SingularityQueueTests
    {
        private SingularityQueue<string> _queue;

        [SetUp]
        public void Setup()
        {
            _queue = new SingularityQueue<string>();
            _queue.Enqueue("abc");
            _queue.Enqueue("def");
            _queue.Enqueue("ghi");
            _queue.Enqueue("jkl");
        }

        public class EnqueueMethod : SingularityQueueTests
        {
            [Test]
            public void Throws_When_Null_Is_Passed()
            {
                Action act = () => _queue.Enqueue(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Test]
            public void Adds_An_Item_To_The_Last_Position()
            {
                _queue.Count.Should().Be(4);
                _queue.Enqueue("xxx");
                _queue.IndexOf("xxx").Should().Be(4);
                _queue.Count.Should().Be(5);
            }
        }

        public class PushInMethod : SingularityQueueTests
        {
            [Test]
            public void Throws_When_Null_Is_Passed()
            {
                Action act = () => _queue.PushIn(null);
                act.Should().Throw<ArgumentNullException>();
            }

            [Test]
            public void Adds_An_Item_To_The_First_Position()
            {
                _queue.Count.Should().Be(4);
                _queue.PushIn("xxx");
                _queue.IndexOf("xxx").Should().Be(0);
                _queue.Count.Should().Be(5);
            }
        }

        public class DequeueMethod : SingularityQueueTests
        {
            [Test]
            public void Returns_Null_When_No_Items_In_Queue()
            {
                _queue.Clear();

                var item = _queue.Dequeue();
                item.Should().BeNull();
            }

            [Test]
            public void Returns_And_Removes_The_First_Item_Per_Call()
            {
                var item = _queue.Dequeue();
                item.Should().Be("abc");

                _queue.Enqueue("~~~");

                item = _queue.Dequeue();
                item.Should().Be("def");

                _queue.PushIn("---");

                item = _queue.Dequeue();
                item.Should().Be("---");

                item = _queue.Dequeue();
                item.Should().Be("ghi");

                _queue.PushIn("***");
                _queue.Enqueue("+++");

                item = _queue.Dequeue();
                item.Should().Be("***");

                item = _queue.Dequeue();
                item.Should().Be("jkl");

                item = _queue.Dequeue();
                item.Should().Be("~~~");

                item = _queue.Dequeue();
                item.Should().Be("+++");

                item = _queue.Dequeue();
                item.Should().BeNull();
            }
        }
    }
}