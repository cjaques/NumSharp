﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using NumSharp.Extensions;
using System.Linq;
using FluentAssertions;
using NumSharp;
using NumSharp.Backends.Unmanaged;

namespace NumSharp.UnitTest
{
    [TestClass]
    public class ShapeTest
    {
        [TestMethod]
        public void Index()
        {
            var shape0 = new Shape(4, 3);

            int idx0 = shape0.GetIndexInShape(2, 1);

            Assert.IsTrue(idx0 == 3 * 2 + 1 * 1);
        }

        [TestMethod]
        public void CheckIndexing()
        {
            var shape0 = new Shape(4, 3, 2);

            int[] strgDimSize = shape0.Strides;

            int index = shape0.GetIndexInShape(1, 2, 1);

            Assert.IsTrue(Enumerable.SequenceEqual(shape0.GetDimIndexOutShape(index), new int[] {1, 2, 1}));

            var rnd = new Randomizer();
            var randomIndex = new int[] {rnd.Next(0, 3), rnd.Next(0, 2), rnd.Next(0, 1)};

            int index1 = shape0.GetIndexInShape(randomIndex);
            Assert.IsTrue(Enumerable.SequenceEqual(shape0.GetDimIndexOutShape(index1), randomIndex));

            var shape1 = new Shape(2, 3, 4);

            index = shape1.GetIndexInShape(1, 2, 1);
            Assert.IsTrue(Enumerable.SequenceEqual(shape1.GetDimIndexOutShape(index), new int[] {1, 2, 1}));

            randomIndex = new int[] {rnd.Next(0, 1), rnd.Next(0, 2), rnd.Next(0, 3)};
            index = shape1.GetIndexInShape(randomIndex);
            Assert.IsTrue(Enumerable.SequenceEqual(shape1.GetDimIndexOutShape(index), randomIndex));

            randomIndex = new int[] {rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10)};

            var shape2 = new Shape(randomIndex);

            randomIndex = new int[] {rnd.Next(0, shape2.Dimensions[0]), rnd.Next(0, shape2.Dimensions[1]), rnd.Next(0, shape2.Dimensions[2])};

            index = shape2.GetIndexInShape(randomIndex);
            Assert.IsTrue(Enumerable.SequenceEqual(shape2.GetDimIndexOutShape(index), randomIndex));
        }

        [TestMethod]
        public void CheckColRowSwitch()
        {
            var shape1 = new Shape(5);
            Assert.IsTrue(Enumerable.SequenceEqual(shape1.Strides, new int[] {1}));

            shape1.ChangeTensorLayout();
            Assert.IsTrue(Enumerable.SequenceEqual(shape1.Strides, new int[] {1}));

            var shape2 = new Shape(4, 3);
            Assert.IsTrue(Enumerable.SequenceEqual(shape2.Strides, new int[] {1, 4}));

            shape2.ChangeTensorLayout();
            Assert.IsTrue(Enumerable.SequenceEqual(shape2.Strides, new int[] {3, 1}));

            var shape3 = new Shape(2, 3, 4);
            Assert.IsTrue(Enumerable.SequenceEqual(shape3.Strides, new int[] {1, 2, 6}));

            shape3.ChangeTensorLayout();
            Assert.IsTrue(Enumerable.SequenceEqual(shape3.Strides, new int[] {12, 4, 1}));
        }

        /// <summary>
        ///     Based on issue https://github.com/SciSharp/NumSharp/issues/306
        /// </summary>
        [TestMethod]
        public void EqualityComparer()
        {
            Shape a = null;
            Shape b = null;

            (a == b).Should().BeTrue();
            (a == null).Should().BeTrue();
            (null == b).Should().BeTrue();

            a = 5;
            b = 4;
            (a != b).Should().BeTrue();

            b = 5;
            (a == b).Should().BeTrue();

            a = new Shape(1, 2, 3, 4, 5);
            b = new Shape(1, 2, 3, 4, 5);
            (a == b).Should().BeTrue();
            b = new Shape(1, 2, 3, 4);
            (a != b).Should().BeTrue();
        }


        [TestMethod, Timeout(10000)]
        public void ExtractShape_FromArray()
        {
            // @formatter:off — disable formatter after this line
            var v = Shape.ExtractShape((Array)new int[][][]
            {
                new int[][]
                {
                    new int[] {1, 2}, new int[] {3, 4}
                }, new int[][]
                {
                    new int[] {5, 6}, new int[] {7, 8}
                }
            });
            // @formatter:on — disable formatter after this line

            v.Should().ContainInOrder(2, 2, 2);
            v.Sum().Should().Be(6);

            v = Shape.ExtractShape(new int[][] {new int[] {1, 2, 3, 4}, new int[] {5, 6, 7, 8}});

            v.Should().ContainInOrder(2, 4);
            v.Sum().Should().Be(6);

            // @formatter:off — disable formatter after this line
            v = Shape.ExtractShape(new int[][][]
            {
                new int[][]
                {
                    new int[] {1, 2}, new int[] {3, 4}
                }, new int[][]
                {
                    new int[] {5, 6}, new int[] {7, 8}
                }
            });
            // @formatter:on — disable formatter after this line

            v.Should().ContainInOrder(2, 2, 2);
            v.Sum().Should().Be(6);

            var jagged = new int[5, 3, 2];
            Shape.ExtractShape(jagged).Should().ContainInOrder(5, 3, 2);
            Shape.ExtractShape(new int[5]).Should().ContainInOrder(5);
        }

        [TestMethod]
        public void Create_Vector()
        {
            Shape.Vector(10).Should().Be(new Shape(10));
            Shape.Vector(10).strides.Should().ContainInOrder(new Shape(10).strides);

            Shape.Vector(1).Should().Be(new Shape(1));
            Shape.Vector(1).strides.Should().ContainInOrder(new Shape(1).strides);

            Shape.Vector(0).Should().Be(new Shape(0));
            Shape.Vector(0).strides.Should().ContainInOrder(new Shape(0).strides);
        }

        [TestMethod]
        public void Create_Matrix()
        {
            Shape.Matrix(5, 5).Should().Be(new Shape(5, 5));
            Shape.Matrix(5, 5).strides.Should().ContainInOrder(new Shape(5, 5).strides);

            Shape.Matrix(1, 5).Should().Be(new Shape(1, 5));
            Shape.Matrix(1, 5).strides.Should().ContainInOrder(new Shape(1, 5).strides);

            Shape.Matrix(5, 1).Should().Be(new Shape(5, 1));
            Shape.Matrix(5, 1).strides.Should().ContainInOrder(new Shape(5, 1).strides);

            Shape.Matrix(5, 0).Should().Be(new Shape(5, 0));
            Shape.Matrix(5, 0).strides.Should().ContainInOrder(new Shape(5, 0).strides);

            Shape.Matrix(0, 0).Should().Be(new Shape(0, 0));
            Shape.Matrix(0, 0).strides.Should().ContainInOrder(new Shape(0, 0).strides);
        }

        [TestMethod]
        public void GetAxis()
        {
            new Shape(10).Slice(":").ViewInfo.Slices[0].Should().Be(new Slice(0, 10, 1));
            new Shape(10).Slice("-77:77").Should().Be(new Shape(10));
            new Shape(10).Slice("-77:77").ViewInfo.Slices[0].Should().Be(new Slice(0, 10, 1));
            new Shape(10).Slice(":7").ViewInfo.Slices[0].Should().Be(new Slice(0, 7, 1));
            new Shape(10).Slice("7:").ViewInfo.Slices[0].Should().Be(new Slice(7, 10, 1));
            new Shape(10).Slice("-7:").ViewInfo.Slices[0].Should().Be(new Slice(3, 10, 1));
            // slice sanitation (prerequisite for shape slicing)
            new Slice("-77:77").Sanitize(10).Should().Be(new Slice(0,10,1));
            new Slice("::77").Sanitize(10).Should().Be(new Slice(0, 10, 77)); //<-- too large step is not to be sanitized, it is just that.

            var baseshape = new Shape(2, 3, 4, 5);
            Shape.GetAxis(baseshape, 0).Should().ContainInOrder(3, 4, 5);
            Shape.GetAxis(baseshape, 1).Should().ContainInOrder(2, 4, 5);
            Shape.GetAxis(baseshape, 2).Should().ContainInOrder(2, 3, 5);
            Shape.GetAxis(baseshape, 3).Should().ContainInOrder(2, 3, 4);
            Shape.GetAxis(baseshape, -1).Should().ContainInOrder(2, 3, 4);
        }

        [TestMethod]
        public void GetSubshape()
        {
            //initialize
            (Shape Shape, int Offset) ret;
            var nd = new NDArray(new ArraySlice<int>(new UnmanagedMemoryBlock<int>(25, 0)), new Shape(5, 5));
            var arr = new int[5, 5];
            var arr2 = new int[5, 1, 5];

            for (int i = 0; i < nd.size; i++)
            {
                nd.Storage.SetIndex(i, i);
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    arr[i, j] = i * 5 + j;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        arr2[i, j, k] = i * 5 + j * 1 + k;
                    }
                }
            }


            //test case 1
            nd.Shape = new Shape(5, 5);

            ret = nd.Shape.GetSubshape(0, 0);
            ret.Shape.Size.Should().Be(1);
            ret.Offset.Should().Be(0);
            arr[0, 0].Should().Be(ret.Offset);

            ret = nd.Shape.GetSubshape(1, 0);
            ret.Shape.Size.Should().Be(1);
            ret.Offset.Should().Be(5);
            arr[1, 0].Should().Be(ret.Offset);

            ret = nd.Shape.GetSubshape(1, 4);
            ret.Shape.Size.Should().Be(1);
            ret.Offset.Should().Be(5 + 4);
            arr[1, 4].Should().Be(ret.Offset);


            //test case 2
            nd.Shape = new Shape(5, 1, 5);
            ret = nd.Shape.GetSubshape(0, 0);
            ret.Shape.Size.Should().Be(5);
            ret.Offset.Should().Be(0);
            arr2[0, 0, 0].Should().Be(ret.Offset);

            ret = nd.Shape.GetSubshape(1, 0);
            ret.Shape.Size.Should().Be(5);
            ret.Offset.Should().Be(5);
            arr2[1, 0, 0].Should().Be(ret.Offset);

            ret = nd.Shape.GetSubshape(1, 0, 1);
            ret.Shape.Size.Should().Be(1);
            ret.Offset.Should().Be(5 + 1);
            arr2[1, 0, 1].Should().Be(ret.Offset);

            ret = nd.Shape.GetSubshape(2, 0, 1);
            ret.Shape.Size.Should().Be(1);
            ret.Offset.Should().Be(5 * 2 + 1);
            arr2[2, 0, 1].Should().Be(ret.Offset);

            ret = nd.Shape.GetSubshape(0, 0);
            ret.Shape.Size.Should().Be(5);
            ret.Offset.Should().Be(0);

            ret = nd.Shape.GetSubshape(1, 0);
            ret.Shape.Size.Should().Be(5);
            ret.Offset.Should().Be(5);

            ret = nd.Shape.GetSubshape(1, 0, 3);
            ret.Shape.Size.Should().Be(1);
            ret.Offset.Should().Be(5 + 3);
            arr2[1, 0, 3].Should().Be(ret.Offset);


            //test case 3
            nd.Shape = new Shape(1, 1, 5, 5);
            ret = nd.Shape.GetSubshape(0, 0, 3, 3);
            ret.Shape.Size.Should().Be(1);
            ret.Offset.Should().Be(18);

            ret = nd.Shape.GetSubshape(0, 0, 3);
            ret.Shape.Size.Should().Be(5);
            ret.Offset.Should().Be(15);

            ret = ret.Shape.GetSubshape(2);
            ret.Shape.Size.Should().Be(1);
            ret.Shape.NDim.Should().Be(0);
            ret.Shape.IsScalar.Should().BeTrue();


            //test case 4
            nd.Shape = new Shape(1, 5, 5, 1);
            ret = nd.Shape.GetSubshape(0, 1);
            ret.Offset.Should().Be(5);
            ret.Shape.NDim.Should().Be(2);
            ret.Shape.Dimensions[0].Should().Be(5);
            ret.Shape.Dimensions[1].Should().Be(1);
        }

        [TestMethod]
        public void RepeatedSlicing()
        {
            new Shape(10).Slice(":").Slice(":").ViewInfo.Slices[0].Should().Be(new Slice(0, 10, 1));
            new Shape(10).Slice(":5").Slice("2:").Slice("::2").ViewInfo.Slices[0].Should().Be(new Slice(2, 5, 2));
        }
    }
}
