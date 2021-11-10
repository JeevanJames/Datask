// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;

namespace Datask.Tool.ExcelData.Core.DbTableSorter
{
    public class TableSorter
    {
        // list of vertices
        private readonly int[] _vertices;

        // adjacency matrix
        private readonly int[,] _matrix;

        private readonly int[] _sortedArray;

        // current number of vertices
        private int _numVertices;

        public TableSorter(int size)
        {
            _vertices = new int[size];
            _matrix = new int[size, size];
            _numVertices = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    _matrix[i, j] = 0;
            }

            // sorted vertex labels
            _sortedArray = new int[size];
        }

        #region - Public Methods -

        public int AddVertex(int vertex)
        {
            _vertices[_numVertices++] = vertex;
            return _numVertices - 1;
        }

        public void AddEdge(int start, int end)
        {
            _matrix[start, end] = 1;
        }

        // Topological sort
        public int[] Sort()
        {
            // while vertices remain,
            while (_numVertices > 0)
            {
                // get a vertex with no successors, or -1
                int currentVertex = NoSuccessors();

                // insert vertex label in sorted array (start at end)
                _sortedArray[_numVertices - 1] = _vertices[currentVertex];

                // delete vertex
                DeleteVertex(currentVertex);
            }

            // vertices all gone; return sortedArray
            return _sortedArray;
        }

        #endregion

        #region - Private Helper Methods

        // returns vertex with no successors (or -1 if no such vertex)
        private int NoSuccessors()
        {
            for (int row = 0; row < _numVertices; row++)
            {
                bool isEdge = false; // edge from row to column in adjMat
                for (int col = 0; col < _numVertices; col++)
                {
                    // if edge to another,
                    if (_matrix[row, col] > 0)
                    {
                        isEdge = true;
                        break; // this vertex has a successor try another
                    }
                }

                if (!isEdge) // if no edges, has no successors
                    return row;
            }

            return 1; // no
        }

        private void DeleteVertex(int delVertex)
        {
            // if not last vertex, delete from vertexList
            if (delVertex != _numVertices - 1)
            {
                for (int j = delVertex; j < _numVertices - 1; j++)
                    _vertices[j] = _vertices[j + 1];

                for (int row = delVertex; row < _numVertices - 1; row++)
                    MoveRowUp(row, _numVertices);

                for (int col = delVertex; col < _numVertices - 1; col++)
                    MoveColLeft(col, _numVertices - 1);
            }

            _numVertices--; // one less vertex
        }

        private void MoveRowUp(int row, int length)
        {
            for (int col = 0; col < length; col++)
                _matrix[row, col] = _matrix[row + 1, col];
        }

        private void MoveColLeft(int col, int length)
        {
            for (int row = 0; row < length; row++)
                _matrix[row, col] = _matrix[row, col + 1];
        }

        #endregion
    }
}
