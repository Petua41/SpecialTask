using SpecialTask;

namespace SpecialTaskTest
{
    class PseudoDequeTest
    {
        [Test]
        public void PushTest()
        {
            LimitedStack<int> deque = new(10)
            {
                1, 2, 3
            };
            LimitedStack<int> expected = new(10)
            {
                1, 2, 3, 4
            };
            deque.Push(4);
            Assert.That(deque, Is.EqualTo(expected));
        }

        [Test]
        public void PopTest()
        {
            LimitedStack<int> deque = new(10)
            {
                1, 2, 3
            };
            LimitedStack<int> expected = new(10)
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
            LimitedStack<int> deque = new(10);
            try { deque.Pop(); }
            catch { throw; }
        }
    }
}
