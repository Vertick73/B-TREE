using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace B_TREE
{
    class Program
    {
        static void Main(string[] args)
        {
            BTree<int> tree = new BTree<int>(10);

            tree.Load(@"test.json");
            //for (int i = 0; i < 10000000; i++)
            //{
            //    tree.Add(i);
            //}




            Console.WriteLine("Hello World!");
            //for (int i = 0; i < 24; i++)
            //{
            //    Console.WriteLine(tree.Contains(i));
            //}
            //tree.Print();
            //tree.Save(@"test.json");
            //tree.Print();
            Console.WriteLine("Hello World!");
        }
    }

    public class BTree<T>:Node<T>
    {
        public Node<T> Root;
        public new void Add(T value)
        {
            if(Root == null)
            {
                Root = new Node<T>(Order) {Owner=this, Leaf=true};
            }
            Root.Add(value);
        }
        public BTree(int order):base(order)
        {
            if (order < 3)
            {
                throw new Exception("Order must be >2");
            }
            Order = order;
        }
        public void Print()
        {
            List<Node<T>> ForPrint = new List<Node<T>> { Root};
            List<Node<T>> ForPrintNext = new List<Node<T>>();
            int i = 0;
            while (true)
            {
                Console.WriteLine($"Leve: {i}");
                foreach (var printNode in ForPrint)
                {
                    foreach (var item in printNode.Nodes)
                    {
                        ForPrintNext.Add(item);
                    }
                    string owner="";
                    foreach (var item in printNode.Owner.Values)
                    {
                        owner += item + " ";
                    }
                    if(owner.Length!=0)
                        owner = owner.Remove(owner.Length - 1);

                    Console.Write($"[{owner}](");
                    string values = "";
                    foreach (var Value in printNode.Values)
                    {
                        values += Value + " ";
                    }
                    if (values.Length != 0)
                        values = values.Remove(values.Length - 1);
                    Console.Write($"{values})\t");
                }
                Console.WriteLine("\n\n");
                if (ForPrintNext.Count == 0)
                    break;
                ForPrint = ForPrintNext;
                ForPrintNext = new List<Node<T>>();
                i++;
            }
        }
        public bool Contains(T value)
        {
            try
            {
                Root.Find(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public new T Find(T value)
        {
            try
            {
                return Root.Find(value);
            }
            catch
            {
                return default(T);
            }
        }
        public void Save(string Path, Formatting formatting = Formatting.Indented)
        {
            using (StreamWriter file = File.CreateText(Path))
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    Formatting = formatting
                };
                serializer.Serialize(file, Root);
            }
        }
        public void Load(string Path)
        {
            Root = JsonConvert.DeserializeObject<Node<T>>(File.ReadAllText(Path));
            Root.Owner = this;
            Node<T> owner = Root;
            OwnerRestore(Root);
        }

        private void OwnerRestore(Node<T> node)
        {
            foreach (var subnode in node.Nodes)
            {
                subnode.Owner = node;
                subnode.Order = Order;
                OwnerRestore(subnode);
            }
        }
           
    }

    public class Node<T>
    {
        [JsonIgnore]
        public int Order { get; set; }
        public bool Leaf { get; set; }
        public List<T> Values { get; set; }
        public List<Node<T>> Nodes { get; set; }
        [JsonIgnore]
        public Node<T> Owner { get; set; }
        public Node(int order)
        {
            Order = order;
            Nodes = new List<Node<T>>();
            Values = new List<T>();
        }
        public void Add(T value)
        {
            if (!Leaf)
            {
                int i;
                for (i = 0; i < Values.Count; i++)
                {
                    if (Comparer<T>.Default.Compare(value, Values[i]) != 1)
                    {
                        Nodes[i].Add(value);
                        return;
                    }
                }
                Nodes[i].Add(value);
            }
            else
            {
                int i;
                for (i = 0; i < Values.Count; i++)
                {
                    if (Comparer<T>.Default.Compare(value, Values[i]) != 1)
                    {
                        Values.Insert(i, value);
                        LeafCheck();
                        return;
                    }
                }
                Values.Insert(i, value);
                LeafCheck();
            }
        }
        private void LeafCheck()
        {
            if (Values.Count == Order)
            {
                List<T> NewElms = new List<T>(Values.GetRange(Order / 2 + 1, Order - (Order / 2 + 1)));
                Node<T> NewNode = new Node<T>(Order) { Values = NewElms, Leaf = true };
                T ValueToInsert = Values[Order / 2];
                Values.RemoveRange(Order / 2, Order - (Order / 2));
                if (Owner is BTree<T>)
                {
                    BTree<T> tree = Owner as BTree<T>;
                    Node<T> NewOwner = new Node<T>(Order) {Owner = tree, Nodes =new List<Node<T>> {this,NewNode },Values=new List<T> { ValueToInsert } };
                    NewNode.Owner = NewOwner;
                    tree.Root = NewOwner;
                    Owner = NewOwner;
                    return;
                }
                Owner.Insert(ValueToInsert, NewNode);
            }
        }
        private void NodeCheck()
        {
            if (Values.Count == Order)
            {
                List<T> NewElms = new List<T>(Values.GetRange(Order / 2 + 1, Order - (Order / 2 + 1)));
                List<Node<T>> NewNodes = new List<Node<T>>(Nodes.GetRange(Order / 2+1, Order - (Order / 2)));
                Node<T> NewNode = new Node<T>(Order) { Nodes = NewNodes, Values = NewElms, Leaf = false };
                foreach (var node in NewNodes)
                {
                    node.Owner = NewNode;
                }
                T ValueToInsert = Values[Order / 2];
                Values.RemoveRange(Order / 2, Order - (Order / 2));
                Nodes.RemoveRange(Order / 2+1, Order - (Order / 2));
                if (Owner is BTree<T>)
                {
                    BTree<T> tree = Owner as BTree<T>;
                    Node<T> NewOwner = new Node<T>(Order) { Owner = tree, Nodes = new List<Node<T>> { this,NewNode }, Values = new List<T> { ValueToInsert } };
                    NewNode.Owner = NewOwner;
                    tree.Root = NewOwner;
                    Owner = NewOwner;
                    return;
                }
                Owner.Insert(ValueToInsert, NewNode);
            }
        }
        private void Insert(T value,Node<T> nodeValue)
        {
            nodeValue.Owner = this;
            int i;
            for (i = 0; i < Values.Count; i++)
            {
                if (Comparer<T>.Default.Compare(value, Values[i]) != 1)
                {
                    Values.Insert(i, value);
                    Nodes.Insert(i+1, nodeValue);
                    NodeCheck();
                    return;
                }
            }
            Values.Insert(i, value);
            Nodes.Insert(i+1, nodeValue);
            NodeCheck();
        }
        public T Find(T value)
        {
            int i;
            for (i = 0; i < Values.Count; i++)
            {
                if (Comparer<T>.Default.Compare(Values[i], value) == 0)
                {
                    return Values[i];
                }
                if (Comparer<T>.Default.Compare(Values[i], value) == 1)
                {
                    if (Leaf)
                    {
                        throw new Exception("Not Found");
                    }
                    return Nodes[i].Find(value);
                }
            }
            if (Leaf)
            {
                throw new Exception("Not Found");
            }
            return Nodes[i].Find(value);
        }
    }
}
