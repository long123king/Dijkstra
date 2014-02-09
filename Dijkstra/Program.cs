using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Dijkstra
{
    class Program
    {
#region NodeInfo
        public class NodeInfo
        {
            private ulong _dis;
            private ulong _name;
            private ulong _parent;

            public ulong Name
            { 
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }

            public ulong Dis
            {
                get
                {
                    return _dis;
                }
                set
                {
                    _dis = value;
                }
            }

            public ulong Parent
            {
                get
                {
                    return _parent;
                }
                set
                {
                    _parent = value;
                }
            }

            public NodeInfo(ulong dis,
                ulong name,
                ulong parent)
            {
                _dis = dis;
                _name = name;
                _parent = parent;
            }
        }
#endregion

#region Program
        private List<ulong> m_weights;
        private ulong m_vertices_num;
        private List<NodeInfo> m_fringe_vertices;

        private Document m_document;
        private PdfWriter m_writer;
        private PdfPTable m_table;

        public Program()
        {
            m_fringe_vertices = new List<NodeInfo>();

            m_document = new Document();
            m_writer = PdfWriter.GetInstance(m_document,
                new FileStream("Dijkstra.pdf", FileMode.Create));

            m_document.Open();
        }

        public void initialize(ulong vertices_num)
        {
            m_vertices_num = vertices_num;
	        m_weights = new List<ulong>((int)(vertices_num * vertices_num));	 

	        for (int i=0;i<(int)m_vertices_num;i++)
	        {
		        for (int j=0;j<(int)m_vertices_num;j++)
		        {
                    if (i == j)
                    {
                        m_weights.Add(ulong.MinValue);
                    }
                    else
                        m_weights.Add(ulong.MaxValue);
		        }

                NodeInfo node = new NodeInfo(ulong.MaxValue, (ulong)i, (ulong)i);
                m_fringe_vertices.Add(node);
	        }

            initializeTable();
        }

        public void finalize()
        {
            finalizeTable();
            m_document.Close();
        }

        public void setWeight(int i, int j, ulong weight)
        {
            m_weights[i * (int)m_vertices_num + j] = weight;
        }

        public void setDoubleWeight(int i, int j, ulong weight)
        {
            setWeight(i, j, weight);
            setWeight(j, i, weight);
        }

        public ulong getWeight(int i, int j)
        {
            return m_weights[i * (int)m_vertices_num + j];
        }

#region PDF Table Writer
        public void initializeTable()
        {
            m_table = new PdfPTable((int)(m_vertices_num + 1));
            m_cells = new List<PdfPCell>();

            for (int i = 0; i < (int)(m_vertices_num + 1);i++ )
            {
                m_cells.Add(new PdfPCell(new Phrase("")));
            }

            AddCellAt(1, "v0");
            AddCellAt(2, "v1");
            AddCellAt(3, "v2");
            AddCellAt(4, "v3");
            AddCellAt(5, "v4");
            AddCellAt(6, "v5");

            resetCells();
        }

        public void AddCell(string content)
        {
            Phrase phrase = new Phrase(content);
            PdfPCell cell = new PdfPCell(phrase);
            m_table.AddCell(cell);
        }

        private List<PdfPCell> m_cells;

        public void AddCellAt(int pos, string content)
        {
            m_cells[pos].Phrase = new Phrase(content);
        }

        public void AddCellAtBold(int pos, string content)
        {
            m_cells[pos].Phrase = new Phrase(content);
            m_cells[pos].Phrase.Font.SetStyle(Font.BOLDITALIC);
        }

        public void resetCells()
        {
            foreach (PdfPCell cell in m_cells)
            {
                m_table.AddCell(cell);
                cell.Phrase = new Phrase("");
                cell.Phrase.Font.SetStyle(Font.NORMAL);
            }
        }

        public void finalizeTable()
        {
            m_document.Add(m_table);
        }
#endregion

#region Node Management
        public NodeInfo findNodeByName(ulong name)
        {
            return m_fringe_vertices.Find(info => info.Name == name);
        }

        public void deleteNodeByName(ulong name)
        {
            m_fringe_vertices.RemoveAll(x => x.Name == name);
        }

        public static int CompareByDis(NodeInfo x, NodeInfo y)
        {
            if (x.Dis < y.Dis)
            {
                return -1;
            } 
            else if (x.Dis > y.Dis)
            {
                return 1;
            }
            else 
            {
                return 0;
            }
        }

        public void sortNodeByDis()
        {
            m_fringe_vertices.Sort(CompareByDis);
        }

        public NodeInfo deleteMinByDis()
        {
            sortNodeByDis();
            NodeInfo minNode = m_fringe_vertices[0];
            m_fringe_vertices.RemoveAt(0);
            return minNode;
        }

#endregion

        public bool isAdjacent(ulong weight)
        {
            return (weight != ulong.MaxValue && weight != ulong.MinValue);
        }

        private int rounds;

        public bool round()
        {
            bool bFinished = false ;

            NodeInfo minNode = deleteMinByDis();

            foreach (NodeInfo fringeNode in m_fringe_vertices)
            {
                ulong weight = getWeight((int)minNode.Name, (int)fringeNode.Name);
                if (isAdjacent(weight))
                {
                    if (fringeNode.Dis > minNode.Dis + weight)
                    {
                        fringeNode.Dis = minNode.Dis + weight;
                    }
                }
            }

            AddCellAt(0, rounds.ToString());
            rounds++;
            string flagged = minNode.Dis + "/v" + minNode.Parent;
            AddCellAtBold((int)(minNode.Name + 1), flagged);
            foreach (NodeInfo fringeNode in m_fringe_vertices)
            {
                if (fringeNode.Dis == ulong.MaxValue)
                {
                    AddCellAt((int)(fringeNode.Name + 1), "Infinity");
                }
                else
                    AddCellAt((int)(fringeNode.Name + 1), fringeNode.Dis.ToString());
            }

            resetCells();

            //string output = "";
            //output += "v";
            //output += minNode.Name;
            //output += "\t";
            //output += minNode.Dis;
            //output += "/v";
            //output += minNode.Parent;

            //Console.WriteLine(output);

            if (m_fringe_vertices.Count == 0)
            {
                bFinished = true;
            }
            return bFinished;
        }

        public void calculate(ulong source)
        {
            NodeInfo sourceNode = findNodeByName(source);
            sourceNode.Dis = 0;
            sourceNode.Parent = source;

            rounds = 1;
            while (!round())
                ;
        }
#endregion

#region main
        static void Main(string[] args)
        {
            Program program = new Program();
            program.initialize(6);

            program.setDoubleWeight(0, 1, 1);
            program.setDoubleWeight(0, 2, 4);
            program.setDoubleWeight(1, 3, 7);
            program.setDoubleWeight(1, 4, 5);
            program.setDoubleWeight(1, 2, 2);
            program.setDoubleWeight(2, 4, 1);
            program.setDoubleWeight(3, 4, 3);
            program.setDoubleWeight(3, 5, 2);
            program.setDoubleWeight(4, 5, 6);

            program.calculate(0);
            program.finalize();

            Process.Start("Dijkstra.pdf");
        }
#endregion

    }
}
