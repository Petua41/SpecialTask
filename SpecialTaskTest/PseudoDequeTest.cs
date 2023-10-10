using SpecialTask;

namespace SpecialTaskTest
{
    class PseudoDequeTest
    {
        [Test]
        public void PushTest()
        {
            PseudoDeque<int> deque = new()
            {
                1, 2, 3
            };
            PseudoDeque<int> expected = new()
            {
                1, 2, 3, 4
            };
            deque.Push(4);
            Assert.That(deque, Is.EqualTo(expected));
        }

        [Test]
        public void PopTest()
        {
            PseudoDeque<int> deque = new()
            {
                1, 2, 3
            };
            PseudoDeque<int> expected = new()
            {
                1, 2
            };
            deque.Pop();
            Assert.That(deque, Is.EqualTo(expected));
        }

        [Test]
        public void PopOnEmptyTest()
        {
            Assert.Throws<UnderflowException>(PopFromEmptyDeque);
        }

        private void PopFromEmptyDeque()
        {
            PseudoDeque<int> deque = new();
            try { deque.Pop(); }
            catch { throw; }
        }

        [Test]
        public void PopBottomTest()
        {
            PseudoDeque<int> deque = new()
            {
                1, 2, 3
            };
            PseudoDeque<int> expected = new()
            {
                2, 3
            };
            deque.PopBottom();
            Assert.That(deque, Is.EqualTo(expected));
        }
    }
}
