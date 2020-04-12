using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace B_TREE
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BTree<int> tree = new BTree<int>(3);

            //tree.Load(@"test.json");
            for (int i = 0; i < 11; i++)
            {
                tree.Add(i);
            }
            tree.Save("tt.json");
            var zz = new BTree<int>(3);
            zz.Load("tt.json");
            zz.Print();

            Console.WriteLine("Hello World!");
            for (int i = 0; i < 24; i++)
            {
                Console.WriteLine(tree.Contains(i));
            }
            //tree.Print();
            //tree.Save(@"test.json");
            //tree.Print();
            Console.WriteLine("Hello World!");
        }
    }

    public class BTree<T> : Node<T>
    {
        public BTree(int order) : base(order)
        {
            if (order < 3)
            {
                throw new Exception("Order must be >2");
            }
        }

        public void Print()
        {
            List<Node<T>> ForPrint = new List<Node<T>> { this };
            List<Node<T>> ForPrintNext = new List<Node<T>>();
            int i = 0;
            while (true)
            {
                Console.WriteLine($"Leve: {i}");
                foreach (var printNode in ForPrint)
                {
                    if (printNode.Nodes != null)
                    {
                        foreach (var item in printNode.Nodes)
                        {
                            ForPrintNext.Add(item);
                        }
                    }
                    string owner = "";
                    if (printNode.Owner != null)
                    {
                        foreach (var item in printNode.Owner.Values)
                        {
                            owner += item + " ";
                        }
                    }

                    if (owner.Length != 0)
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

        public void Save(string Path, Formatting formatting = Formatting.Indented)
        {
            using (StreamWriter file = File.CreateText(Path))
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    Formatting = formatting
                };
                serializer.Serialize(file, this);
            }
        }

        public void Load(string Path)
        {
            Node<T> LoadedData = JsonConvert.DeserializeObject<Node<T>>(File.ReadAllText(Path));
            Values = LoadedData.Values;
            Nodes = LoadedData.Nodes;
            Order = LoadedData.Order;
            OwnerRestore(this);
        }

        private void OwnerRestore(Node<T> node)
        {
            if (node.Nodes == null)
                return;
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

        public List<T> Values { get; set; }
        public List<Node<T>> Nodes { get; set; }

        [JsonIgnore]
        public Node<T> Owner { get; set; }

        public Node(int order)
        {
            Order = order;
        }

        public void Add(T value)
        {
            if (Nodes != null)
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
                if (Values == null)
                {
                    Values = new List<T>();
                }
                for (i = 0; i < Values.Count; i++)
                {
                    if (Comparer<T>.Default.Compare(value, Values[i]) != 1)
                    {
                        Values.Insert(i, value);
                        Check();
                        return;
                    }
                }
                Values.Insert(i, value);
                Check();
            }
        }

        private void Check()
        {
            if (Values.Count == Order)
            {
                Node<T> NewNode = new Node<T>(Order);
                T ValueToInsert = Values[Order / 2];
                List<T> NewElms = new List<T>(Values.GetRange(Order / 2 + 1, Order - (Order / 2 + 1)));
                NewNode.Values = NewElms;
                Values.RemoveRange(Order / 2, Order - (Order / 2));

                if (Nodes != null)
                {
                    List<Node<T>> NewNodes = new List<Node<T>>(Nodes.GetRange(Order / 2 + 1, Order - (Order / 2)));
                    foreach (var node in NewNodes)
                    {
                        node.Owner = NewNode;
                    }
                    NewNode.Nodes = NewNodes;
                    Nodes.RemoveRange(Order / 2 + 1, Order - (Order / 2));
                }
                if (Owner == null)
                {
                    NewNode.Owner = this;
                    Node<T> NewNode2 = new Node<T>(Order) { Owner = this, Nodes = Nodes, Values = Values };
                    Values = new List<T> { ValueToInsert };
                    Nodes = new List<Node<T>> { NewNode2, NewNode };
                    return;
                }
                Owner.Insert(ValueToInsert, NewNode);
            }
        }

        private void Insert(T value, Node<T> nodeValue)
        {
            nodeValue.Owner = this;
            int i;
            for (i = 0; i < Values.Count; i++)
            {
                if (Comparer<T>.Default.Compare(value, Values[i]) != 1)
                {
                    Values.Insert(i, value);
                    Nodes.Insert(i + 1, nodeValue);
                    Check();
                    return;
                }
            }
            Values.Insert(i, value);
            Nodes.Insert(i + 1, nodeValue);
            Check();
        }

        public bool Contains(T value)
        {
            int i;
            for (i = 0; i < Values.Count; i++)
            {
                if (Comparer<T>.Default.Compare(Values[i], value) == 0)
                {
                    return true;
                }
                if (Comparer<T>.Default.Compare(Values[i], value) == 1)
                {
                    if (Nodes == null)
                    {
                        return false;
                    }
                    return Nodes[i].Contains(value);
                }
            }
            if (Nodes == null)
            {
                return false;
            }
            return Nodes[i].Contains(value);
        }
    }
}