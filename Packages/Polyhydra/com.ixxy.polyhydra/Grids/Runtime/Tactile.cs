/*
 * Tactile-JS
 * Copyright 2018 Craig S. Kaplan, csk@uwaterloo.ca
 *
 * Distributed under the terms of the 3-clause BSD license.  See the
 * file "LICENSE" for more information.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EdgeShape
{
    J, // Edges can be a path of any shape
    U, // Edges must look the same after reflecting across their length
    S, // Edges must look the same after a 180° rotation
    I  // Edges must look the same after both rotation and reflection
};

public struct TilingTypeConfig
{
    public string tiling_name;
    public int num_params;
    public int num_aspects;
    public int num_vertices;
    public int num_edge_shapes;
    public EdgeShape[] edge_shapes;
    public bool[] edge_orientations;
    public int[] edge_shape_ids;
    public double[] default_params;
    public double[] vertex_coeffs;
    public double[] translation_coeffs;
    public double[] aspect_coeffs;
    public int[] colouring;

    public TilingTypeConfig(string tiling_name, int num_params, int num_aspects, int num_vertices, int num_edge_shapes,
        EdgeShape[] edge_shapes, bool[] edge_orientations,
        int[] edge_shape_ids, double[] default_params, double[] vertex_coeffs,
        double[] translation_coeffs, double[] aspect_coeffs, int[] colouring)
    {
        this.tiling_name = tiling_name;
        this.num_params = num_params;
        this.num_aspects = num_aspects;
        this.num_vertices = num_vertices;
        this.num_edge_shapes = num_edge_shapes;
        this.edge_shapes = edge_shapes;
        this.edge_orientations = edge_orientations;
        this.edge_shape_ids = edge_shape_ids;
        this.default_params = default_params;
        this.vertex_coeffs = vertex_coeffs;
        this.translation_coeffs = translation_coeffs;
        this.aspect_coeffs = aspect_coeffs;
        this.colouring = colouring;
    }
}

public class IsohedralTilingHelpers
{

    static EdgeShape[] es_00 = {EdgeShape.J, EdgeShape.J, EdgeShape.J};
    static EdgeShape[] es_01 = {EdgeShape.S, EdgeShape.J, EdgeShape.S, EdgeShape.S, EdgeShape.S};
    static EdgeShape[] es_02 = {EdgeShape.S, EdgeShape.J, EdgeShape.J, EdgeShape.S};
    static EdgeShape[] es_03 = {EdgeShape.S, EdgeShape.J, EdgeShape.S, EdgeShape.J};
    static EdgeShape[] es_04 = {EdgeShape.S, EdgeShape.S, EdgeShape.S};
    static EdgeShape[] es_05 = {EdgeShape.S, EdgeShape.J};
    static EdgeShape[] es_06 = {EdgeShape.J};
    static EdgeShape[] es_07 = {EdgeShape.S};
    static EdgeShape[] es_08 = {EdgeShape.U, EdgeShape.J};
    static EdgeShape[] es_09 = {EdgeShape.U, EdgeShape.S, EdgeShape.S};
    static EdgeShape[] es_10 = {EdgeShape.J, EdgeShape.I};
    static EdgeShape[] es_11 = {EdgeShape.S, EdgeShape.I, EdgeShape.S};
    static EdgeShape[] es_12 = {EdgeShape.I, EdgeShape.J};
    static EdgeShape[] es_13 = {EdgeShape.I, EdgeShape.S};
    static EdgeShape[] es_14 = {EdgeShape.U};
    static EdgeShape[] es_15 = {EdgeShape.I};
    static EdgeShape[] es_16 = {EdgeShape.S, EdgeShape.J, EdgeShape.J};
    static EdgeShape[] es_17 = {EdgeShape.J, EdgeShape.J, EdgeShape.I};
    static EdgeShape[] es_18 = {EdgeShape.S, EdgeShape.S, EdgeShape.J, EdgeShape.S};
    static EdgeShape[] es_19 = {EdgeShape.S, EdgeShape.S, EdgeShape.J, EdgeShape.I};
    static EdgeShape[] es_20 = {EdgeShape.J, EdgeShape.J, EdgeShape.S};
    static EdgeShape[] es_21 = {EdgeShape.S, EdgeShape.I, EdgeShape.I};
    static EdgeShape[] es_22 = {EdgeShape.J, EdgeShape.I, EdgeShape.I};
    static EdgeShape[] es_23 = {EdgeShape.J, EdgeShape.J};
    static EdgeShape[] es_24 = {EdgeShape.I, EdgeShape.I};
    static EdgeShape[] es_25 = {EdgeShape.J, EdgeShape.S};
    static EdgeShape[] es_26 = {EdgeShape.S, EdgeShape.S, EdgeShape.S, EdgeShape.S};
    static EdgeShape[] es_27 = {EdgeShape.J, EdgeShape.S, EdgeShape.S};
    static EdgeShape[] es_28 = {EdgeShape.I, EdgeShape.S, EdgeShape.I, EdgeShape.S};
    static EdgeShape[] es_29 = {EdgeShape.J, EdgeShape.I, EdgeShape.S};
    static EdgeShape[] es_30 = {EdgeShape.I, EdgeShape.I, EdgeShape.I, EdgeShape.S};
    static EdgeShape[] es_31 = {EdgeShape.S, EdgeShape.S};
    static EdgeShape[] es_32 = {EdgeShape.S, EdgeShape.I};
    static EdgeShape[] es_33 = {EdgeShape.U, EdgeShape.I};
    static EdgeShape[] es_34 = {EdgeShape.U, EdgeShape.S};
    static EdgeShape[] es_35 = {EdgeShape.I, EdgeShape.I, EdgeShape.I};
    static EdgeShape[] es_36 = {EdgeShape.I, EdgeShape.S, EdgeShape.I};
    static EdgeShape[] es_37 = {EdgeShape.I, EdgeShape.S, EdgeShape.S};

    static int[] esi_00 = {0, 1, 2, 0, 1, 2};
    static int[] esi_01 = {0, 0, 1, 2, 2, 1};
    static int[] esi_02 = {0, 1, 0, 2, 1, 2};
    static int[] esi_03 = {0, 1, 2, 3, 1, 4};
    static int[] esi_04 = {0, 1, 2, 2, 1, 3};
    static int[] esi_05 = {0, 1, 2, 3, 1, 3};
    static int[] esi_06 = {0, 0, 1, 1, 2, 2};
    static int[] esi_07 = {0, 1, 1, 0, 1, 1};
    static int[] esi_08 = {0, 0, 0, 0, 0, 0};
    static int[] esi_09 = {0, 1, 2, 0, 2, 1};
    static int[] esi_10 = {0, 1, 0, 0, 1, 0};
    static int[] esi_11 = {0, 1, 2, 2, 1, 0};
    static int[] esi_12 = {0, 1, 1, 1, 1, 0};
    static int[] esi_13 = {0, 1, 1, 2, 2};
    static int[] esi_14 = {0, 0, 1, 2, 1};
    static int[] esi_15 = {0, 1, 2, 3, 2};
    static int[] esi_16 = {0, 1, 2, 1, 2};
    static int[] esi_17 = {0, 1, 1, 1, 1};
    static int[] esi_18 = {0, 1, 2, 0};
    static int[] esi_19 = {0, 1, 1, 0};
    static int[] esi_20 = {0, 0, 0, 0};
    static int[] esi_21 = {0, 1, 0};
    static int[] esi_22 = {0, 1, 0, 1};
    static int[] esi_23 = {0, 1, 0, 2};
    static int[] esi_24 = {0, 0, 1, 1};
    static int[] esi_25 = {0, 1, 2, 3};
    static int[] esi_26 = {0, 0, 1, 2};
    static int[] esi_27 = {0, 1, 2};
    static int[] esi_28 = {0, 0, 1};
    static int[] esi_29 = {0, 0, 0};

    static bool[] eo_00 = {false, false, false, false, false, false, false, true, false, true, false, true};
    static bool[] eo_01 = {false, false, true, true, false, false, false, false, true, true, false, true};
    static bool[] eo_02 = {false, false, false, false, true, true, false, false, false, true, true, true};
    static bool[] eo_03 = {false, false, false, false, false, false, false, false, false, true, false, false};
    static bool[] eo_04 = {false, false, false, false, false, false, true, true, false, true, false, false};
    static bool[] eo_05 = {false, false, false, false, false, false, false, false, true, true, true, true};
    static bool[] eo_06 = {false, false, false, true, false, false, false, true, false, false, false, true};
    static bool[] eo_07 = {false, false, false, false, false, false, false, false, false, false, false, false};
    static bool[] eo_08 = {false, false, false, false, true, true, false, false, false, false, true, true};
    static bool[] eo_09 = {false, false, false, false, true, true, false, true, false, true, true, false};
    static bool[] eo_10 = {false, false, false, false, false, false, false, true, true, false, true, false};
    static bool[] eo_11 = {false, false, false, false, true, true, false, true, true, false, true, false};
    static bool[] eo_12 = {false, false, false, false, false, false, true, false, true, false, true, false};
    static bool[] eo_13 = {false, false, false, false, false, true, true, true, true, false, true, false};
    static bool[] eo_14 = {false, false, false, false, true, false, false, false, false, false, true, false};
    static bool[] eo_15 = {false, false, false, false, false, true, false, false, false, true};
    static bool[] eo_16 = {false, false, true, true, false, false, false, false, false, true};
    static bool[] eo_17 = {false, false, false, false, false, false, false, false, false, true};
    static bool[] eo_18 = {false, false, true, false, false, false, false, false, true, false};
    static bool[] eo_19 = {false, false, false, false, false, false, true, true, true, true};
    static bool[] eo_20 = {false, false, false, false, false, true, true, true, true, false};
    static bool[] eo_21 = {false, false, false, false, false, false, false, true};
    static bool[] eo_22 = {false, false, false, false, false, true, false, true};
    static bool[] eo_23 = {false, false, false, false, true, false, true, false};
    static bool[] eo_24 = {false, false, false, true, false, false, false, true};
    static bool[] eo_25 = {false, false, true, false, true, true, false, true};
    static bool[] eo_26 = {false, false, true, false, false, false, true, false};
    static bool[] eo_27 = {false, false, false, false, false, true};
    static bool[] eo_28 = {false, false, false, false, true, false};
    static bool[] eo_29 = {false, false, false, false, false, true, false, false};
    static bool[] eo_30 = {false, false, false, false, false, true, true, true};
    static bool[] eo_31 = {false, false, true, true, false, false, true, true};
    static bool[] eo_32 = {false, false, false, false, true, true, false, false};
    static bool[] eo_33 = {false, false, false, false, false, false, false, false};
    static bool[] eo_34 = {false, false, false, false, true, true, true, true};
    static bool[] eo_35 = {false, false, true, true, false, false, false, false};
    static bool[] eo_36 = {false, false, false, true, false, false, false, false};
    static bool[] eo_37 = {false, false, false, false, false, true, true, false};
    static bool[] eo_38 = {false, false, false, false, true, false, false, false};
    static bool[] eo_39 = {false, false, true, true, false, true, true, false};
    static bool[] eo_40 = {false, false, false, true, true, true, true, false};
    static bool[] eo_41 = {false, false, false, false, false, false};
    static bool[] eo_42 = {false, false, true, true, false, false};
    static bool[] eo_43 = {false, false, false, true, false, false};
    static bool[] eo_44 = {false, false, true, false, false, false};

    static double[] dp_00 = {0.12239750492, 0.5, 0.143395479017, 0.625};
    static double[] dp_01 = {0.12239750492, 0.5, 0.225335752741, 0.225335752741};
    static double[] dp_02 = {0.12239750492, 0.5, 0.225335752741, 0.625};
    static double[] dp_03 = {0.12239750492, 0.5, 0.315470053838, 0.5, 0.315470053838, 0.5};
    static double[] dp_04 = {0.12239750492, 0.5, 0.225335752741, 0.225335752741, 0.5};
    static double[] dp_05 = {0.12239750492, 0.5, 0.225335752741, 0.625, 0.5};
    static double[] dp_06 = {0.6, 0.196416770201};
    static double[] dp_07 = {0.12239750492, 0.5, 0.225335752741};
    static double[] dp_08 = { };
    static double[] dp_09 = {0.12239750492, 0.225335752741};
    static double[] dp_10 = {0.12239750492, 0.225335752741, 0.5};
    static double[] dp_11 = {0.12239750492, 0.225335752741, 0.225335752741};
    static double[] dp_12 = {0.216506350946};
    static double[] dp_13 = {0.104512294489, 0.65};
    static double[] dp_14 = {0.230769230769, 0.5, 0.225335752741};
    static double[] dp_15 = {0.230769230769, 0.5, 0.225335752741, 0.5};
    static double[] dp_16 = {0.230769230769, 0.225335752741};
    static double[] dp_17 = {0.141304, 0.465108, 0.534891};
    static double[] dp_18 = {0.452827026611, 0.5};
    static double[] dp_19 = {0.366873818946};
    static double[] dp_20 = {0.230769230769};
    static double[] dp_21 = {0.230769230769, 0.5};
    static double[] dp_22 = {0.5, 0.102564102564};
    static double[] dp_23 = {0.230769230769, 0.869565217391};
    static double[] dp_24 = {0.5, 0.230769230769, 0.5, 0.5};
    static double[] dp_25 = {0.230769230769, 0.5, 0.230769230769};
    static double[] dp_26 = {0.5, 0.5, 0.6};
    static double[] dp_27 = {0.5, 0.102564102564, 0.102564102564};
    static double[] dp_28 = {0.230769230769, 0.230769230769};
    static double[] dp_29 = {0.5};
    static double[] dp_30 = {0.105263157895};
    static double[] dp_31 = {0.196416770201};
    static double[] dp_32 = {0.5, 0.196416770201};

    static double[] tvc_00 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0, 0.1, 0, 5, 0, 0, -2.5, 3.9, 0, 5.5, 0, -0.4, 0, 5, 0, -4, 0.5, 3.9,
        0, 0, 0, 0.1, 0, 5, 0, 0, -1.5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -5.5, 0, 0.5, 0, 0, 0, 4, -2
    };

    static double[] tvc_01 =
    {
        3.9, 0, 0, 0, 0.1, 0, 5, 0, 0, -2.5, 3.9, 0, 0, 3.5, -0.4, 0, 5, 0, 0, -2, 3.9, 0, 0, 0, 0.1, 0, 5, 0, 0, -1.5,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -3.5, 0, 0.5, 0, 0, 0, 0, 0.5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    static double[] tvc_02 =
    {
        0, 0, -3.5, 0, 0.5, 0, 0, 0, 4, -2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0, 0.1, 0, 5, 0, 0, -2.5, 3.9, 0,
        3.5, 0, -0.4, 0, 5, 0, 4, -4.5, 3.9, 0, 0, 0, 0.1, 0, 5, 0, 0, -1.5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1
    };

    static double[] tvc_03 =
    {
        0, 0, -2.5, 0, 0, 0, 0.5, 0, 0, 0, 3, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0, 0, 0,
        0.1, 0, 5, 0, 0, 0, 0, -2.5, 3.9, 0, 0, 0, 2.5, 0, -0.4, 0, 5, 0, 0, 0, 3, -3.5, 3.9, 0, 0, 0, 0, 0, 0.1, 0, 5,
        0, 0, 0, 0, -1.5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1
    };

    static double[] tvc_04 =
    {
        3.9, 0, 0, 3.5, 0, -0.4, 0, 5, 0, 0, 5, -4.5, 3.9, 0, 0, 0, 0, 0.1, 0, 5, 0, 0, 0, -1.5, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 1, 0, 0, -3.5, 0, 0, 0.5, 0, 0, 0, 0, 0, 0.5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0, 0, 0.1,
        0, 5, 0, 0, 0, -2.5
    };

    static double[] tvc_05 =
    {
        3.9, 0, 3.5, 0, 0, -0.4, 0, -5, 0, 4, 0, 0.5, 3.9, 0, 0, 0, 5, -2.4, 0, 5, 0, 0, 0, -1.5, 0, 0, 0, 0, 5, -2.5,
        0, 0, 0, 0, 0, 1, 0, 0, -3.5, 0, 0, 0.5, 0, 0, 0, 4, 0, -2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0, 0,
        0.1, 0, -5, 0, 0, 0, 2.5
    };

    static double[] tvc_06 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0, 0, -0.288675134595, 0, 0, 1, 0, 0, 0, 2.5, 1.12583302492, -0.721132486541,
        -1.44337567297, 1.95, 1.06036297108, 5, 0, -2.5, 0, 3.9, 0.1, 2.5, -1.12583302492, -1.27886751346,
        1.44337567297, 1.95, -0.671687836487
    };

    static double[] tvc_07 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0.1, 0, 5, 0, -2.5, 3.9, 0, 3.5, -0.4, 0, 5, 0, -2, 3.9, 0, 0, 0.1, 0, 5, 0,
        -1.5, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -3.5, 0.5, 0, 0, 0, 0.5
    };

    static double[] tvc_08 =
    {
        1, 0, 0.5, 0.866025403784, -0.5, 0.866025403784, -1, 0, -0.5, -0.866025403784, 0.5, -0.866025403784
    };

    static double[] tvc_09 =
    {
        0, 0, 0, 0, 0, 0, 3.9, 0, 0.1, 0, 0, 0, 3.9, 3.5, -0.4, 0, 0, 0.5, 3.9, 0, 0.1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0,
        -3.5, 0.5, 0, 0, 0.5
    };

    static double[] tvc_10 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0.1, 0, 0, 0, 0, 3.9, 3.5, 0, -0.4, 0, 0, 5, -2, 3.9, 0, 0, 0.1, 0, 0, 0, 1,
        0, 0, 0, 0, 0, 0, 0, 1, 0, -3.5, 0, 0.5, 0, 0, 5, -2
    };

    static double[] tvc_11 =
    {
        3.9, 3.5, -0.4, 0, 0, 0.5, 3.9, 0, 0.1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, -3.5, 0.5, 0, 0, 0.5, 0, 0, 0, 0, 0, 0,
        3.9, 0, 0.1, 0, 0, 0
    };

    static double[] tvc_12 =
    {
        0, -3.5, 0, 0.5, 0, 0, 0, 0.5, 0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0.1, 0, 0, 0, 0, 3.9, 0, 3.5, -0.4, 0, 0, 0,
        0.5, 3.9, 0, 0, 0.1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1
    };

    static double[] tvc_13 =
    {
        0, 0.5, 0, -0.288675134595, 0, 1, 0, 0, 1.15470053838, 0.75, 2, 0.144337567297, 0, 0.5, 4, 0, -1.15470053838,
        0.25, 2, 0.144337567297, 0, 0, 0, 0
    };

    static double[] tvc_14 =
    {
        0, 0, 1, 0, 0, 0, 0, 5, -2.5, 5.1, 0, -0.1, -1.47224318643, 2.5, -1.22113248654, 2.55, 1.44337567297,
        -0.771687836487, 0, 0, 0, 0, 0, 0, 0, 0, 0.5, 0, 0, -0.866025403784
    };

    static double[] tvc_15 =
    {
        3.9, 0, 0, 0.1, 0, 5, 0, -2.5, 3.9, 0, 3.5, -0.4, 0, 5, 0, -2, 3.9, 0, 0, 0.1, 0, 5, 0, -1.5, 0, 0, 0, 0, 0, 0,
        0, 1, 0, 0, 0, 0, 0, 0, 0, 0
    };

    static double[] tvc_16 =
    {
        3.9, 0, 0, 0, 0.1, 0, 5, 0, 0, -2.5, 3.9, 0, 3.5, 0, -0.4, 0, 5, 0, 4, -4, 3.9, 0, 0, 0, 0.1, 0, 5, 0, 0, -1.5,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    static double[] tvc_17 =
    {
        3.9, 0, 0.1, 0, 0, 0, 3.9, 3.5, -0.4, 0, 0, 0.5, 3.9, 0, 0.1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0
    };

    static double[] tvc_18 =
    {
        0, 0, 5, -2.5, 0, 0, 0, 1, 0, 0, -5, 2.5, 0, 10, 0, -4, 0, 0, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0.1, 0, -5, 0, 2.5,
        3.9, 0, 5, -2.4, 0, 5, 0, -1.5
    };

    static double[] tvc_19 =
    {
        0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1.95, 2.5, -0.95, -1.95, 2.5, -1.05, 3.9, 0, 0.1, 0, 5, -2, 1.95, -2.5,
        1.55, 1.95, 2.5, -0.45
    };

    static double[] tvc_20 =
    {
        0, -1, 0, 0, 0, 1, 0, 0, 4.95, 0.55, 4.95, 0.55, 0, 0, 9.9, 0.1, -4.95, -0.55, 4.95, 0.55
    };

    static double[] tvc_21 =
    {
        0, 1, 0, 0, 2.925, 0.075, 1.68874953738, 0.0433012701892, 0, 0, 0, 0, -2.925, 1.425, 1.68874953738,
        -0.822724133595
    };

    static double[] tvc_22 =
    {
        1, 0, 0.75, 0.433012701892, 0, 0, 0.75, -0.433012701892
    };

    static double[] tvc_23 =
    {
        0.5, 0, 0, 0.866025403784, -0.5, 0, 0, -0.866025403784
    };

    static double[] tvc_24 =
    {
        0, 0.57735026919, -1, 0, 1, 0
    };

    static double[] tvc_25 =
    {
        0, 0, 0, 0, 0, 0, 3.9, 0, 0.1, 0, 5, -2.5, 3.9, 0, 0.1, 0, 5, -1.5, 0, 0, 0, 0, 0, 1
    };

    static double[] tvc_26 =
    {
        5, 0, -2, 0, -3.9, -0.1, 0, 0, 1, 0, 0, 0, 5, 0, -2, 0, 3.9, 0.1, 0, 0, 0, 0, 0, 0
    };

    static double[] tvc_27 =
    {
        0, 0, 1, 0, 0, 0, 0, -3.45, 4, 3.9, 0, 0.1, 0, 3.45, -3, 3.9, 0, 0.1, 0, 0, 0, 0, 0, 0
    };

    static double[] tvc_28 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 5, 0, 0, 0, -1.5, 0, 3.9, 0, 0, 0.1, 0, 0, 5, 0,
        -2.5, 0, 0, 0, 5, -1.5
    };

    static double[] tvc_29 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -5, 3.9, 2.6, 3.9, 0, 0, 0.1, 0, -5, 0, 2.5, 3.9, 0, 0, 0.1
    };

    static double[] tvc_30 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, -10, 0, 0, 5, 0, 10, 0, -4, 10, 0, 10, -10, 0, 10, 0, -5, 0, 0, 10, -5
    };

    static double[] tvc_31 =
    {
        0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 3.9, 0.1, 0, 0, 3.9, 0.1
    };

    static double[] tvc_32 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0.5, 0, 0, 3.9, 0.1, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, -2, 0, -3.9, 0, -0.1
    };

    static double[] tvc_33 =
    {
        0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 3.9, 0, 0.1, 0, 0, 0, 3.9, 0, 0.1, 0, 3.9, 0.1
    };

    static double[] tvc_34 =
    {
        1, 0, 1, 1, 0, 1, 0, 0
    };

    static double[] tvc_35 =
    {
        1.8, 0.1, 0, 0, 0, 1, 0, 1, 0, 0, -1.8, 1.9, 0, 0, 0, 0
    };

    static double[] tvc_36 =
    {
        3.8, 0.1, 0, 0, 0, 0, -3.8, 0.9, -3.8, -0.1, 0, 0, 0, 0, 3.8, -0.9
    };

    static double[] tvc_37 =
    {
        0, 0, 0.57735026919, 0, 0, 1
    };

    static double[] tvc_38 =
    {
        0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 3.9, 0.1
    };

    static double[] tvc_39 =
    {
        0.5, 0.5, 0, 0, 1, 0
    };

    static double[] tvc_40 =
    {
        0, 1, 0, 0, 0, 0.5, 3.9, 0.1, 0, 0, 0, 0
    };

    static double[] tvc_41 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 5, 0, -2, 0, 3.9, 0.1
    };

    static double[] tvc_42 =
    {
        1, 0, -0.5, 0.866025403784, -0.5, -0.866025403784
    };

    static double[] tc_00 = {0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 3.9, 0, 5.5, 0, -0.4, 0, 5, 0, -4, -0.5};
    static double[] tc_01 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 7.8, 0, 3.5, 3.5, -0.8, 0, 0, 0, 0, 0};
    static double[] tc_02 = {0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -7.8, 0, -7, 0, 0.8, 0, 0, 0, 0, -1};

    static double[] tc_03 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -7.8, 0, -2.5, 0, -2.5, 0, 0.8, 0, -10, 0, 3, 0, -3, 4};

    static double[] tc_04 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, -15.6, 0, -7, -7, 0, 1.6, 0, 0, 0, 0, 0, -2};
    static double[] tc_05 = {0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, -3, 7.8, 0, 7, 0, 0, -0.8, 0, 0, 0, 0, 0, 0};

    static double[] tc_06 =
    {
        -2.5, -3.37749907476, 0.663397459622, 4.33012701892, -1.95, -3.08108891325, -2.5, 3.37749907476, 2.33660254038,
        -4.33012701892, -1.95, 2.11506350946
    };

    static double[] tc_07 = {0, 0, 0, 0, 0, 0, 0, -1, 7.8, 0, 7, -0.8, 0, 0, 0, 0};
    static double[] tc_08 = {1.5, 0.866025403784, 1.5, -0.866025403784};
    static double[] tc_09 = {1.5, 0.866025403784, 0, 1.73205080757};
    static double[] tc_10 = {0, 0, 0, 0, 0, -1, 3.9, 3.5, -0.4, 0, 0, -0.5};
    static double[] tc_11 = {0, 0, 0, 0, 0, 0, 0, -1, 7.8, 7, 0, -0.8, 0, 0, 0, 0};
    static double[] tc_12 = {3.9, 3.5, -0.4, 0, 0, 0.5, 3.9, 3.5, -0.4, 0, 0, -0.5};
    static double[] tc_13 = {0, 0, 0, 0, 0, 0, 0, -1, -7.8, -3.5, -3.5, 0.8, 0, 0, 0, 0};
    static double[] tc_14 = {0, 0, -4, -0.866025403784, 3.46410161514, 0.75, -2, -0.433012701892};
    static double[] tc_15 ={4.4167295593, -2.5, 2.66339745962, -2.55, -4.33012701892, 1.34903810568, 0, -5, 2.5, -5.1, 0, -1.63205080757};
    static double[] tc_16 = {-7.8, 0, -3.5, 0.3, 0, 0, 0, -0.5, -7.8, 0, -3.5, 0.3, 0, 0, 0, 0.5};
    static double[] tc_17 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 7.8, 0, 3.5, 0, -0.3, 0, 10, 0, 4, -7.5};
    static double[] tc_18 = {0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 15.6, 0, 7, 0, -0.6, 0, 0, 0, 0, 0};
    static double[] tc_19 = {0, 0, 0, 0, 0, 0, 0, 1, -15.6, 0, -7, 0.6, 0, 0, 0, 0};
    static double[] tc_20 = {0, 0, 0, 0, 0, 1, -7.8, -3.5, 0.3, 0, 0, 0.5};
    static double[] tc_21 = {0, 0, 0, 0, 0, 10, 0, -3, -7.8, 0, -10, 4.8, 0, 0, 0, 0};
    static double[] tc_22 = {-3.9, 5, -3.1, -3.9, -5, 1.9, -3.9, -5, 1.9, 3.9, -5, 3.1};
    static double[] tc_23 = {9.9, 1.1, -9.9, -1.1, -9.9, -1.1, -9.9, -1.1};
    static double[] tc_24 = {0, 0, 0, 1.73205080757, 0, 1.5, 0, -0.866025403784};
    static double[] tc_25 = {-1.5, 0.866025403784, -1.5, -0.866025403784};
    static double[] tc_26 = {0, 1.73205080757, 1.5, -0.866025403784};
    static double[] tc_27 = {-1, 1.73205080757, 1, 1.73205080757};
    static double[] tc_28 = {1, 1.73205080757, -1, 1.73205080757};
    static double[] tc_29 = {1, 1.73205080757, 2, 0};
    static double[] tc_30 = {0, 0, 0, 0, 0, -1, 3.9, 0, 0.1, 0, 5, -2.5};
    static double[] tc_31 = {0, 0, 0, 0, 0, -1, 7.8, 0, 0.2, 0, 0, 0};
    static double[] tc_32 = {0, 0, 0, 0, -7.8, -0.2, 0, 0, 1, 0, 0, 0};
    static double[] tc_33 = {0, -6.9, 8, 0, 0, 0, 0, -3.45, 4, -3.9, 0, -0.1};
    static double[] tc_34 = {-5, 0, -5, 0, 5, 0, -3.9, 0, -5, 1.4, -5, 0, 0, 0, 1.5, 0, -3.9, 0, 0, -0.1};
    static double[] tc_35 = {0, 0, 0, 0, 0, -1, 7.8, 0, 0.2, 0, 10, -5};
    static double[] tc_36 = {0, 0, 0, 0, -7.8, 0, 0, -0.2, 0, 0, 3.9, 1.1, 0, 0, 0, 0};
    static double[] tc_37 = {-15.6, 0, -0.4, 0, 0, 0, 0, 0, 0, 0, 0, -1};
    static double[] tc_38 = {0, 0, 0, 0, -20, 0, -20, 20, 0, 0, 0, -2, 0, 0, 0, 0};
    static double[] tc_39 = {0, 2, 0, 0, 0, 0, -7.8, -0.2};
    static double[] tc_40 = {0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -7.8, -7.8, -0.4};
    static double[] tc_41 = {-7.8, 0, -0.2, 0, 0, 0, -3.9, 0, -0.1, 0, 3.9, 1.1};
    static double[] tc_42 = {0, 2, 2, 0};
    static double[] tc_43 = {0, 0, 0, 4, 0, -2, 0, 2};
    static double[] tc_44 = {0, 0, -7.6, 1.8, 7.6, 0.2, -7.6, 1.8};
    static double[] tc_45 = {1, 1, 1, -1};
    static double[] tc_46 = {1, 0, 0, 1};
    static double[] tc_47 = {0, 0, -3.9, -0.1, 0, 1, 0, 0};
    static double[] tc_48 = {0, 0, -3.9, -0.1, 0, 2, 0, 0};
    static double[] tc_49 = {0, -3.45, 4, -3.9, 0, -0.1, 0, -3.45, 4, 3.9, 0, 0.1};
    static double[] tc_50 = {3.8, 0.1, -3.8, 0.9, -3.8, -0.1, -3.8, 0.9};
    static double[] tc_51 = {0, 2, -1.73205080757, 1};
    static double[] tc_52 = {0, 2, 0, 0, 0, 1, 3.9, 0.1};
    static double[] tc_53 = {0, 1, -1, 0};
    static double[] tc_54 = {-1, 1, -2, 0};
    static double[] tc_55 = {0, 1, 1, 0};
    static double[] tc_56 = {0, 0.5, -3.9, -0.1, 0, -0.5, -3.9, -0.1};
    static double[] tc_57 = {-5, 0, 2, 0, -3.9, -0.1, -5, 0, 3, 0, -3.9, -0.1};
    static double[] tc_58 = {0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 7.8, 0.2};
    static double[] tc_59 = {0, 1, 0, 0, 0, 0, 7.8, 0.2};
    static double[] tc_60 = {-1.5, 2.59807621135, -3, 0};
    static double[] tc_61 = {0, -0.5, -3.9, -0.1, 0, 0.5, -3.9, -0.1};

    static double[] ac_00 =
    {
        0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0
    };

    static double[] ac_01 =
    {
        0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0,
        0, 0, 0, 7.8, 0, 0, 3.5, -0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, -0.5
    };

    static double[] ac_02 =
    {
        0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0,
        0, 0, 0, -3.9, 0, -3.5, 0, 0.4, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 5, 0, 4, -4.5
    };

    static double[] ac_03 =
    {
        0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -2.5, 0, 0, 0, 0.5, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, -1, 0, 0, 0, 3, 0, 0, -1
    };

    static double[] ac_04 =
    {
        0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 7.8, 0, 0, 3.5, 0, -0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 10, 0, 0, 5,
        -6, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, -3.5, 0, 0, 0.5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
        0, -0.5, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -7.8, 0, -3.5, -3.5, 0, 0.8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1,
        0, 10, 0, 0, 5, -7.5
    };

    static double[] ac_05 =
    {
        0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 7.8, 0, 3.5, 0, 5, -2.8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 4, 0,
        -1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 3.9, 0, 0, 0, 5, -2.4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 5, 0, 0,
        0, -1.5, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 3.9, 0, 3.5, 0, 0, -0.4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0,
        -5, 0, 4, 0, 0.5
    };

    static double[] ac_06 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -0.5, 0, 0, -0.866025403784, 0, 0, 0.5, 0, 0,
        0.866025403784, 0, 0, -0.5, 0, 0, -0.866025403784, 0, 0, -0.5, 0, 0, 0.866025403784, 0, 0, 1, 0, 0,
        -0.866025403784, 0, 0, -0.5, 0, 0, 0
    };

    static double[] ac_07 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 7.8, 0, 3.5,
        -0.3, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -0.5
    };

    static double[] ac_08 =
    {
        1, 0, 0, 0, 1, 0
    };

    static double[] ac_09 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0
    };

    static double[] ac_10 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 7.8, 3.5, 0,
        -0.3, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 5, -2
    };

    static double[] ac_11 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, -3.5, 0,
        0.5, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0.5
    };

    static double[] ac_12 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0.5, 0, 0.866025403784, 0, 0.5, 0, 0.866025403784, 0, -0.5, 0,
        -0.866025403784, 0, -0.5, 0, -0.866025403784, 0, 0.5, 0, 0.866025403784, 0, -0.5, 0, -0.866025403784
    };

    static double[] ac_13 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0.5, 0, 0, 0.866025403784, 0, 0, 1, 0, 0,
        -0.866025403784, 0, 0, 0.5, 0, 0, 0, 0, 0, -0.5, 0, 0, 0.866025403784, 0, 0, 1.5, 0, 0, -0.866025403784, 0, 0,
        -0.5, 0, 0, -0.866025403784, 0, 0, -1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, -1.73205080757, 0, 0, -0.5, 0,
        0, -0.866025403784, 0, 0, 0, 0, 0, 0.866025403784, 0, 0, -0.5, 0, 0, -1.73205080757, 0, 0, 0.5, 0, 0,
        -0.866025403784, 0, 0, -0.5, 0, 0, 0.866025403784, 0, 0, 0.5, 0, 0, -0.866025403784
    };

    static double[] ac_14 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0
    };

    static double[] ac_15 =
    {
        0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0,
        0, 0, 0, 7.8, 0, 3.5, 0, -0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 10, 0, 4, -6.5
    };

    static double[] ac_16 =
    {
        0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0,
        0, 0, 0, 7.8, 0, 3.5, 0, -0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 10, 0, 4, -6.5, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0,
        15.6, 0, 7, 0, -0.6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 7.8, 0, 3.5, 0,
        -0.3, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 10, 0, 4, -6.5
    };

    static double[] ac_17 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 0, -7.8, 0, -3.5, 0.3, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0,
        -0.5, 0, 0, 0, 1, 0, 0, 0, 0, -7.8, 0, -3.5, 0.3, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0.5
    };

    static double[] ac_18 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 1
    };

    static double[] ac_19 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, -1, 0, 10, 0, -3, 0, 0, 0, 1, 0, 0, 0, 0, -3.9, 0, -5, 2.4, 0, 0, 0, 0, 0, 0, 0, -1, 0, 5, 0,
        -1.5, 0, 0, 0, -1, 0, 0, 0, 0, 3.9, 0, 5, -2.4, 0, 0, 0, 0, 0, 0, 0, 1, 0, 5, 0, -1.5
    };

    static double[] ac_20 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0,
        0, 0, 0, 0, -1, -3.9, 0, -0.1, 0, 0, 1, 0, 0, 0, 0, -5, 3, 0, 0, 0, 0, 0, 1, -3.9, 0, -1.1, 0, 0, -1, 0, 0, 0,
        0, -5, 3
    };

    static double[] ac_21 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, -1, 9.9, 1.1, 0, 1, 0, 0, 0,
        0, 0, 0, 0, 1, -9.9, -1.1, 0, 1, 0, 0, 0, 0
    };

    static double[] ac_22 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -0.5, 0, 0.866025403784, 0, 1.5, 0, -0.866025403784, 0, -0.5, 0,
        0.866025403784, 0, -0.5, 0, -0.866025403784, 0, 1.5, 0, 0.866025403784, 0, -0.5, 0, -0.866025403784, 0, 0.5, 0,
        0.866025403784, 0, 0, 0, 0.866025403784, 0, -0.5, 0, 0, 0, -1, 0, 0, 0, 1.5, 0, 0, 0, 1, 0, 0.866025403784, 0,
        0.5, 0, -0.866025403784, 0, 0, 0, -0.866025403784, 0, -0.5, 0, 1.73205080757
    };

    static double[] ac_23 =
    {
        1, 0, 0, 0, 1, 0, 0.5, -0.866025403784, 0, 0.866025403784, 0.5, 0, -0.5, -0.866025403784, 0, 0.866025403784,
        -0.5, 0, -1, 0, 0, 0, -1, 0, -0.5, 0.866025403784, 0, -0.866025403784, -0.5, 0, 0.5, 0.866025403784, 0,
        -0.866025403784, 0.5, 0
    };

    static double[] ac_24 =
    {
        1, 0, 0, 0, 1, 0, -0.5, -0.866025403784, 1.5, 0.866025403784, -0.5, -0.866025403784, -0.5, 0.866025403784, 1.5,
        -0.866025403784, -0.5, 0.866025403784, 0.5, 0.866025403784, 0, 0.866025403784, -0.5, 0, 0.5, -0.866025403784, 0,
        -0.866025403784, -0.5, 1.73205080757, -1, 0, 1.5, 0, 1, 0.866025403784
    };

    static double[] ac_25 =
    {
        1, 0, 0, 0, 1, 0, -0.5, 0.866025403784, 0.75, -0.866025403784, -0.5, 0.433012701892, -0.5, -0.866025403784,
        0.75, 0.866025403784, -0.5, -0.433012701892
    };

    static double[] ac_26 =
    {
        1, 0, 0, 0, 1, 0, 0.5, -0.866025403784, 0.75, 0.866025403784, 0.5, 0.433012701892, -0.5, -0.866025403784, 0.75,
        0.866025403784, -0.5, 1.29903810568
    };

    static double[] ac_27 =
    {
        1, 0, 0, 0, 1, 0, 0.5, 0.866025403784, 0.75, 0.866025403784, -0.5, 0.433012701892, -0.5, -0.866025403784, 0.75,
        0.866025403784, -0.5, -0.433012701892
    };

    static double[] ac_28 =
    {
        1, 0, 0, 0, 1, 0, -0.5, -0.866025403784, 0.75, -0.866025403784, 0.5, 0.433012701892, -0.5, 0.866025403784, 0.75,
        0.866025403784, 0.5, -0.433012701892
    };

    static double[] ac_29 =
    {
        1, 0, 0, 0, 1, 0, -0.5, 0.866025403784, -0.5, -0.866025403784, -0.5, 0.866025403784, -0.5, -0.866025403784, 0.5,
        0.866025403784, -0.5, 0.866025403784, -0.5, 0.866025403784, -1.5, 0.866025403784, 0.5, 0.866025403784, -0.5,
        -0.866025403784, -0.5, -0.866025403784, 0.5, 0.866025403784, 1, 0, -1, 0, -1, 1.73205080757
    };

    static double[] ac_30 =
    {
        1, 0, 0, 0, 1, 0, -0.5, 0.866025403784, -0.5, -0.866025403784, -0.5, 0.866025403784, -0.5, -0.866025403784, 0.5,
        0.866025403784, -0.5, 0.866025403784, 0.5, -0.866025403784, -0.5, 0.866025403784, 0.5, 0.866025403784, 0.5,
        0.866025403784, -1.5, -0.866025403784, 0.5, 0.866025403784, -1, 0, -1, 0, -1, 1.73205080757
    };

    static double[] ac_31 =
    {
        1, 0, 0, 0, 1, 0, -0.5, -0.866025403784, 0.5, 0.866025403784, -0.5, 0.866025403784, -0.5, 0.866025403784, -0.5,
        -0.866025403784, -0.5, 0.866025403784, 0.5, 0.866025403784, 0.5, -0.866025403784, 0.5, 0.866025403784, 0.5,
        -0.866025403784, 1.5, 0.866025403784, 0.5, 0.866025403784, -1, 0, 1, 0, -1, 1.73205080757
    };

    static double[] ac_32 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 7.8, 0, 0.2, 0, 0, 0, 0, 0, 1, 0, 0, 0
    };

    static double[] ac_33 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3.9, 0, 0.1, 0, 0, 0, 0, 0, -1, 0, 5,
        -1.5
    };

    static double[] ac_34 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 5, 0, -1, 0, 0, 0, 0, 0, 1, 0, -3.9,
        -0.1
    };

    static double[] ac_35 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, -3.45, 4, 0, 0, 0, 0, 0, -1, 3.9, 0,
        0.1
    };

    static double[] ac_36 =
    {
        0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0
    };

    static double[] ac_37 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 7.8, 0, 0.2, 0, 0, 0, 0, 0, -1, 0, 10,
        -4
    };

    static double[] ac_38 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, -5, 3.9,
        3.6, 0, 0, 0, 0, 0, 0, 0, -1, 3.9, 0, 0, 0.1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0,
        0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, -5, 3.9, 3.6, 0, 0, 0, 0, 0, 0, 0, 1, 3.9, 0, 0, 0.1
    };

    static double[] ac_39 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 1, 0,
        0, -1, 0, 0, 0, 7.8, 0, 0.2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, -7.8, 0, -0.2, 0, 0, 0, 0, 0, -1, 0,
        0, 1
    };

    static double[] ac_40 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 10, 0, -5,
        0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 10, -5, 0, 0, 0, -1, 0, 0, 0, 0, 0, 10, 0, -4, 0, 0, 0, 0, 0, 0, 0, 1, -10, 0,
        -10, 10, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, -1, -10, 0, 0, 5
    };

    static double[] ac_41 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 2, 0, 0, 0, -1, 0, 0, 0, -1, 0, 0, 0, 1, 0, 0, 0, 1, -3.9,
        -0.1, 0, 1, 0, 0, 0, 1, 0, 0, 0, -1, 3.9, 0.1
    };

    static double[] ac_42 =
    {
        0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 5, 0, 0, -2, 0,
        0, 0, 0, 0, 0, 0, -1, 0, -3.9, 0, -0.1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0.5, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0,
        3.9, 0.1, 0, 0, 0, -1, 0, 0, 0, 0, 5, 0, 0, -2.5, 0, 0, 0, 0, 0, 0, 0, 1, 0, -3.9, -3.9, -0.2
    };

    static double[] ac_43 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 3.9, 0, 0.1, 0, 0, 0, 0, 0, -1, 0, 3.9,
        1.1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, -3.9, 0, -0.1, 0, 0, 0, 0, 0, -1,
        0, 3.9, 1.1
    };

    static double[] ac_44 =
    {
        1, 0, 0, 0, 1, 0, 0, 1, 0, -1, 0, 2, -1, 0, 2, 0, -1, 2, 0, -1, 2, 1, 0, 0
    };

    static double[] ac_45 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 2, 0, -1, 0, 0, 0, 2, 0, 0, 0, -1, 0, 2,
        0, 0, 0, -1, 0, 2, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 4, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0,
        -1, 0, 0, 0, 2, 0, 0, 0, 1, 0, 2, 0, 0, 0, -1, 0, 2, 0, -1, 0, 0, 0, 4
    };

    static double[] ac_46 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 3.8, 0.1, 0, 0, 0, -1, -3.8, 0.9
    };

    static double[] ac_47 =
    {
        1, 0, 0, 0, 1, 0, 0, -1, 2, 1, 0, 0
    };

    static double[] ac_48 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0
    };

    static double[] ac_49 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 2, 0, 0, 0, -1, 3.9, 0.1
    };

    static double[] ac_50 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, -3.45, 5, 0, 0, 0, 0, 0, -1, 3.9, 0,
        0.1
    };

    static double[] ac_51 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 5, 0, -1, 0, 0, 0, 0, 0, -1, 0, -3.9,
        -0.1
    };

    static double[] ac_52 =
    {
        1, 0, 0, 0, 1, 0, 0, -1, 2, 1, 0, 0, -1, 0, 2, 0, -1, 2, 0, 1, 0, -1, 0, 2
    };

    static double[] ac_53 =
    {
        1, 0, 0, 0, 1, 0, 0.5, 0.866025403784, -0.866025403784, -0.866025403784, 0.5, 0.5, -0.5, 0.866025403784,
        -0.866025403784, -0.866025403784, -0.5, 1.5, -1, 0, 0, 0, -1, 2, -0.5, -0.866025403784, 0.866025403784,
        0.866025403784, -0.5, 1.5, 0.5, -0.866025403784, 0.866025403784, 0.866025403784, 0.5, 0.5, -1, 0, 0, 0, 1, 0,
        -0.5, 0.866025403784, -0.866025403784, 0.866025403784, 0.5, 0.5, 0.5, 0.866025403784, -0.866025403784,
        0.866025403784, -0.5, 1.5, 1, 0, 0, 0, -1, 2, 0.5, -0.866025403784, 0.866025403784, -0.866025403784, -0.5, 1.5,
        -0.5, -0.866025403784, 0.866025403784, -0.866025403784, 0.5, 0.5
    };

    static double[] ac_54 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 1, 0, 0, 0, -1, 3.9, 0.1, 0, -1, 0, 0, 0, 2, 0, 0, 0, 1, 0,
        0, 0, 1, 0, 0, 0, 1, 0, 0, 0, -1, 3.9, 0.1
    };

    static double[] ac_55 =
    {
        1, 0, 0, 0, 1, 0, 0, 1, 0, -1, 0, 1, -1, 0, 1, 0, -1, 1, 0, -1, 1, 1, 0, 0
    };

    static double[] ac_56 =
    {
        1, 0, 0, 0, 1, 0, 0, 1, 0, -1, 0, 1, -1, 0, 1, 0, -1, 1, 0, -1, 1, 1, 0, 0, -1, 0, 0, 0, 1, 0, 0, -1, 0, -1, 0,
        1, 1, 0, -1, 0, -1, 1, 0, 1, -1, 1, 0, 0
    };

    static double[] ac_57 =
    {
        1, 0, 0, 0, 1, 0, 0, -1, 1, 1, 0, 0, -1, 0, 1, 0, -1, 1, 0, 1, 0, -1, 0, 1
    };

    static double[] ac_58 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0
    };

    static double[] ac_59 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0
    };

    static double[] ac_60 =
    {
        0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, 0, 0, 5, 0, -1, 0, 0, 0, 0, 0, -1, 0, 3.9,
        0.1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 7.8, 0.2, 0, 0, -1, 0, 0, 0, 5, 0, -1, 0, 0, 0, 0, 0, 1,
        0, 3.9, 0.1
    };

    static double[] ac_61 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 1, 0, 0, 0, -1, 7.8, 0.2, 0, 1, 0, 0, 0, 0.5, 0, 0, 0, -1,
        3.9, 0.1, 0, -1, 0, 0, 0, 1.5, 0, 0, 0, 1, 3.9, 0.1
    };

    static double[] ac_62 =
    {
        1, 0, 0, 0, 1, 0, 0.5, -0.866025403784, 0.5, 0.866025403784, 0.5, 0.866025403784, -0.5, -0.866025403784, 0,
        0.866025403784, -0.5, 1.73205080757, -1, 0, -1, 0, -1, 1.73205080757, -0.5, 0.866025403784, -1.5,
        -0.866025403784, -0.5, 0.866025403784, 0.5, 0.866025403784, -1, -0.866025403784, 0.5, 0
    };

    static double[] ac_63 =
    {
        1, 0, 0, 0, 1, 0, -1, 0, 0.5, 0, -1, 0.866025403784
    };

    static double[] ac_64 =
    {
        0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0, 0, 1, 0, 0, 0, -1, 0, 0
    };

    static int[] c_00 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 2, 0, 1, 3};
    static int[] c_01 = {0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 1, 2, 3};
    static int[] c_02 = {0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 1, 2, 0, 1, 3};
    static int[] c_03 = {0, 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 1, 2, 0, 1, 3};
    static int[] c_04 = {0, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 1, 2, 3};
    static int[] c_05 = {0, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 3};
    static int[] c_06 = {0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 1, 0, 1, 2, 3};
    static int[] c_07 = {0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 2, 0, 2, 0, 1, 3};
    static int[] c_08 = {0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 2, 0, 1, 3};
    static int[] c_09 = {0, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 1, 1, 2, 0, 3};
    static int[] c_10 = {0, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 1, 0, 1, 2, 3};
    static int[] c_11 = {0, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 2, 0, 3};
    static int[] c_12 = {0, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 1, 2, 3};
    static int[] c_13 = {0, 1, 2, 1, 2, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 2, 0, 1, 3};
    static int[] c_14 = {0, 1, 2, 0, 1, 2, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 3};
    static int[] c_15 = {0, 2, 1, 1, 0, 2, 0, 0, 0, 0, 0, 0, 1, 2, 0, 2, 0, 1, 3};
    static int[] c_16 = {0, 2, 1, 0, 1, 2, 0, 0, 0, 0, 0, 0, 2, 0, 1, 1, 2, 0, 3};
    static int[] c_17 = {1, 0, 2, 2, 0, 1, 0, 0, 0, 0, 0, 0, 1, 2, 0, 2, 0, 1, 3};
    static int[] c_18 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 1, 0, 2, 2};
    static int[] c_19 = {0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 0, 1, 2, 2};
    static int[] c_20 = {0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 2};
    static int[] c_21 = {0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 2};
    static int[] c_22 = {0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 0, 2, 2};
    static int[] c_23 = {0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 2};
    static int[] c_24 = {0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 2};
    static int[] c_25 = {0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 2};
    static int[] c_26 = {0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 2, 0, 1, 2, 2};
    static int[] c_27 = {0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 1, 0, 2, 2};
    static int[] c_28 = {0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 2, 2};


    public static TilingTypeConfig[] tiling_types = new[]
    {

        // IH00 is undefined
        new TilingTypeConfig(),

        // IH01
        new TilingTypeConfig(
            tiling_name: "IH01",
            num_params: 4,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 3,
            edge_shapes: es_00,
            edge_orientations: eo_00,
            edge_shape_ids: esi_00,
            default_params: dp_00,
            vertex_coeffs: tvc_00,
            translation_coeffs: tc_00,
            aspect_coeffs: ac_00,
            colouring: c_00
        ),

        // IH02
        new TilingTypeConfig(
            tiling_name: "IH02",
            num_params: 4,
            num_aspects: 2,
            num_vertices: 6,
            num_edge_shapes: 3,
            edge_shapes: es_00,
            edge_orientations: eo_01,
            edge_shape_ids: esi_01,
            default_params: dp_01,
            vertex_coeffs: tvc_01,
            translation_coeffs: tc_01,
            aspect_coeffs: ac_01,
            colouring: c_01
        ),

        // IH03
        new TilingTypeConfig(
            tiling_name: "IH03",
            num_params: 4,
            num_aspects: 2,
            num_vertices: 6,
            num_edge_shapes: 3,
            edge_shapes: es_00,
            edge_orientations: eo_02,
            edge_shape_ids: esi_02,
            default_params: dp_02,
            vertex_coeffs: tvc_02,
            translation_coeffs: tc_02,
            aspect_coeffs: ac_02,
            colouring: c_02
        ),

        // IH04
        new TilingTypeConfig(
            tiling_name: "IH04",
            num_params: 6,
            num_aspects: 2,
            num_vertices: 6,
            num_edge_shapes: 5,
            edge_shapes: es_01,
            edge_orientations: eo_03,
            edge_shape_ids: esi_03,
            default_params: dp_03,
            vertex_coeffs: tvc_03,
            translation_coeffs: tc_03,
            aspect_coeffs: ac_03,
            colouring: c_02
        ),

        // IH05
        new TilingTypeConfig(
            tiling_name: "IH05",
            num_params: 5,
            num_aspects: 4,
            num_vertices: 6,
            num_edge_shapes: 4,
            edge_shapes: es_02,
            edge_orientations: eo_04,
            edge_shape_ids: esi_04,
            default_params: dp_04,
            vertex_coeffs: tvc_04,
            translation_coeffs: tc_04,
            aspect_coeffs: ac_04,
            colouring: c_03
        ),

        // IH06
        new TilingTypeConfig(
            tiling_name: "IH06",
            num_params: 5,
            num_aspects: 4,
            num_vertices: 6,
            num_edge_shapes: 4,
            edge_shapes: es_03,
            edge_orientations: eo_05,
            edge_shape_ids: esi_05,
            default_params: dp_05,
            vertex_coeffs: tvc_05,
            translation_coeffs: tc_05,
            aspect_coeffs: ac_05,
            colouring: c_04
        ),

        // IH07
        new TilingTypeConfig(
            tiling_name: "IH07",
            num_params: 2,
            num_aspects: 3,
            num_vertices: 6,
            num_edge_shapes: 3,
            edge_shapes: es_00,
            edge_orientations: eo_06,
            edge_shape_ids: esi_06,
            default_params: dp_06,
            vertex_coeffs: tvc_06,
            translation_coeffs: tc_06,
            aspect_coeffs: ac_06,
            colouring: c_05
        ),

        // IH08
        new TilingTypeConfig(
            tiling_name: "IH08",
            num_params: 4,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 3,
            edge_shapes: es_04,
            edge_orientations: eo_07,
            edge_shape_ids: esi_00,
            default_params: dp_00,
            vertex_coeffs: tvc_00,
            translation_coeffs: tc_00,
            aspect_coeffs: ac_00,
            colouring: c_00
        ),

        // IH09
        new TilingTypeConfig(
            tiling_name: "IH09",
            num_params: 3,
            num_aspects: 2,
            num_vertices: 6,
            num_edge_shapes: 2,
            edge_shapes: es_05,
            edge_orientations: eo_08,
            edge_shape_ids: esi_07,
            default_params: dp_07,
            vertex_coeffs: tvc_07,
            translation_coeffs: tc_07,
            aspect_coeffs: ac_07,
            colouring: c_06
        ),

        // IH10
        new TilingTypeConfig(
            tiling_name: "IH10",
            num_params: 0,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 1,
            edge_shapes: es_06,
            edge_orientations: eo_06,
            edge_shape_ids: esi_08,
            default_params: dp_08,
            vertex_coeffs: tvc_08,
            translation_coeffs: tc_08,
            aspect_coeffs: ac_08,
            colouring: c_00
        ),

        // IH11
        new TilingTypeConfig(
            tiling_name: "IH11",
            num_params: 0,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 1,
            edge_shapes: es_07,
            edge_orientations: eo_07,
            edge_shape_ids: esi_08,
            default_params: dp_08,
            vertex_coeffs: tvc_08,
            translation_coeffs: tc_09,
            aspect_coeffs: ac_08,
            colouring: c_00
        ),

        // IH12
        new TilingTypeConfig(
            tiling_name: "IH12",
            num_params: 2,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 2,
            edge_shapes: es_08,
            edge_orientations: eo_09,
            edge_shape_ids: esi_07,
            default_params: dp_09,
            vertex_coeffs: tvc_09,
            translation_coeffs: tc_10,
            aspect_coeffs: ac_09,
            colouring: c_00
        ),

        // IH13
        new TilingTypeConfig(
            tiling_name: "IH13",
            num_params: 3,
            num_aspects: 2,
            num_vertices: 6,
            num_edge_shapes: 3,
            edge_shapes: es_09,
            edge_orientations: eo_10,
            edge_shape_ids: esi_09,
            default_params: dp_10,
            vertex_coeffs: tvc_10,
            translation_coeffs: tc_11,
            aspect_coeffs: ac_10,
            colouring: c_06
        ),

        // IH14
        new TilingTypeConfig(
            tiling_name: "IH14",
            num_params: 2,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 2,
            edge_shapes: es_10,
            edge_orientations: eo_11,
            edge_shape_ids: esi_10,
            default_params: dp_09,
            vertex_coeffs: tvc_11,
            translation_coeffs: tc_12,
            aspect_coeffs: ac_09,
            colouring: c_00
        ),

        // IH15
        new TilingTypeConfig(
            tiling_name: "IH15",
            num_params: 3,
            num_aspects: 2,
            num_vertices: 6,
            num_edge_shapes: 3,
            edge_shapes: es_11,
            edge_orientations: eo_12,
            edge_shape_ids: esi_11,
            default_params: dp_11,
            vertex_coeffs: tvc_12,
            translation_coeffs: tc_13,
            aspect_coeffs: ac_11,
            colouring: c_06
        ),

        // IH16
        new TilingTypeConfig(
            tiling_name: "IH16",
            num_params: 1,
            num_aspects: 3,
            num_vertices: 6,
            num_edge_shapes: 2,
            edge_shapes: es_12,
            edge_orientations: eo_13,
            edge_shape_ids: esi_12,
            default_params: dp_12,
            vertex_coeffs: tvc_13,
            translation_coeffs: tc_14,
            aspect_coeffs: ac_12,
            colouring: c_05
        ),

        // IH17
        new TilingTypeConfig(
            tiling_name: "IH17",
            num_params: 2,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 2,
            edge_shapes: es_13,
            edge_orientations: eo_14,
            edge_shape_ids: esi_07,
            default_params: dp_09,
            vertex_coeffs: tvc_09,
            translation_coeffs: tc_10,
            aspect_coeffs: ac_09,
            colouring: c_00
        ),

        // IH18
        new TilingTypeConfig(
            tiling_name: "IH18",
            num_params: 0,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 1,
            edge_shapes: es_14,
            edge_orientations: eo_06,
            edge_shape_ids: esi_08,
            default_params: dp_08,
            vertex_coeffs: tvc_08,
            translation_coeffs: tc_09,
            aspect_coeffs: ac_08,
            colouring: c_00
        ),

        // IH19 is undefined
        new TilingTypeConfig(),

        // IH20
        new TilingTypeConfig(
            tiling_name: "IH20",
            num_params: 0,
            num_aspects: 1,
            num_vertices: 6,
            num_edge_shapes: 1,
            edge_shapes: es_15,
            edge_orientations: eo_07,
            edge_shape_ids: esi_08,
            default_params: dp_08,
            vertex_coeffs: tvc_08,
            translation_coeffs: tc_09,
            aspect_coeffs: ac_08,
            colouring: c_00
        ),

        // IH21
        new TilingTypeConfig(
            tiling_name: "IH21",
            num_params: 2,
            num_aspects: 6,
            num_vertices: 5,
            num_edge_shapes: 3,
            edge_shapes: es_16,
            edge_orientations: eo_15,
            edge_shape_ids: esi_13,
            default_params: dp_13,
            vertex_coeffs: tvc_14,
            translation_coeffs: tc_15,
            aspect_coeffs: ac_13,
            colouring: c_07
        ),

        // IH22
        new TilingTypeConfig(
            tiling_name: "IH22",
            num_params: 3,
            num_aspects: 2,
            num_vertices: 5,
            num_edge_shapes: 3,
            edge_shapes: es_17,
            edge_orientations: eo_16,
            edge_shape_ids: esi_14,
            default_params: dp_14,
            vertex_coeffs: tvc_15,
            translation_coeffs: tc_16,
            aspect_coeffs: ac_14,
            colouring: c_06
        ),

        // IH23
        new TilingTypeConfig(
            tiling_name: "IH23",
            num_params: 4,
            num_aspects: 2,
            num_vertices: 5,
            num_edge_shapes: 4,
            edge_shapes: es_18,
            edge_orientations: eo_17,
            edge_shape_ids: esi_15,
            default_params: dp_15,
            vertex_coeffs: tvc_16,
            translation_coeffs: tc_17,
            aspect_coeffs: ac_15,
            colouring: c_08
        ),

        // IH24
        new TilingTypeConfig(
            tiling_name: "IH24",
            num_params: 4,
            num_aspects: 4,
            num_vertices: 5,
            num_edge_shapes: 4,
            edge_shapes: es_19,
            edge_orientations: eo_17,
            edge_shape_ids: esi_15,
            default_params: dp_15,
            vertex_coeffs: tvc_16,
            translation_coeffs: tc_18,
            aspect_coeffs: ac_16,
            colouring: c_09
        ),

        // IH25
        new TilingTypeConfig(
            tiling_name: "IH25",
            num_params: 3,
            num_aspects: 4,
            num_vertices: 5,
            num_edge_shapes: 3,
            edge_shapes: es_20,
            edge_orientations: eo_16,
            edge_shape_ids: esi_14,
            default_params: dp_14,
            vertex_coeffs: tvc_15,
            translation_coeffs: tc_19,
            aspect_coeffs: ac_17,
            colouring: c_10
        ),

        // IH26
        new TilingTypeConfig(
            tiling_name: "IH26",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 5,
            num_edge_shapes: 3,
            edge_shapes: es_21,
            edge_orientations: eo_18,
            edge_shape_ids: esi_14,
            default_params: dp_16,
            vertex_coeffs: tvc_17,
            translation_coeffs: tc_20,
            aspect_coeffs: ac_18,
            colouring: c_01
        ),

        // IH27
        new TilingTypeConfig(
            tiling_name: "IH27",
            num_params: 3,
            num_aspects: 4,
            num_vertices: 5,
            num_edge_shapes: 3,
            edge_shapes: es_16,
            edge_orientations: eo_19,
            edge_shape_ids: esi_16,
            default_params: dp_17,
            vertex_coeffs: tvc_18,
            translation_coeffs: tc_21,
            aspect_coeffs: ac_19,
            colouring: c_11
        ),

        // IH28
        new TilingTypeConfig(
            tiling_name: "IH28",
            num_params: 2,
            num_aspects: 4,
            num_vertices: 5,
            num_edge_shapes: 3,
            edge_shapes: es_16,
            edge_orientations: eo_15,
            edge_shape_ids: esi_13,
            default_params: dp_18,
            vertex_coeffs: tvc_19,
            translation_coeffs: tc_22,
            aspect_coeffs: ac_20,
            colouring: c_12
        ),

        // IH29
        new TilingTypeConfig(
            tiling_name: "IH29",
            num_params: 1,
            num_aspects: 4,
            num_vertices: 5,
            num_edge_shapes: 2,
            edge_shapes: es_12,
            edge_orientations: eo_20,
            edge_shape_ids: esi_17,
            default_params: dp_19,
            vertex_coeffs: tvc_20,
            translation_coeffs: tc_23,
            aspect_coeffs: ac_21,
            colouring: c_04
        ),

        // IH30
        new TilingTypeConfig(
            tiling_name: "IH30",
            num_params: 1,
            num_aspects: 6,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_22,
            edge_orientations: eo_21,
            edge_shape_ids: esi_18,
            default_params: dp_20,
            vertex_coeffs: tvc_21,
            translation_coeffs: tc_24,
            aspect_coeffs: ac_22,
            colouring: c_13
        ),

        // IH31
        new TilingTypeConfig(
            tiling_name: "IH31",
            num_params: 0,
            num_aspects: 6,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_23,
            edge_orientations: eo_22,
            edge_shape_ids: esi_19,
            default_params: dp_08,
            vertex_coeffs: tvc_22,
            translation_coeffs: tc_25,
            aspect_coeffs: ac_23,
            colouring: c_14
        ),

        // IH32
        new TilingTypeConfig(
            tiling_name: "IH32",
            num_params: 0,
            num_aspects: 6,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_24,
            edge_orientations: eo_23,
            edge_shape_ids: esi_19,
            default_params: dp_08,
            vertex_coeffs: tvc_22,
            translation_coeffs: tc_26,
            aspect_coeffs: ac_24,
            colouring: c_15
        ),

        // IH33
        new TilingTypeConfig(
            tiling_name: "IH33",
            num_params: 0,
            num_aspects: 3,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_23,
            edge_orientations: eo_22,
            edge_shape_ids: esi_19,
            default_params: dp_08,
            vertex_coeffs: tvc_23,
            translation_coeffs: tc_08,
            aspect_coeffs: ac_25,
            colouring: c_05
        ),

        // IH34
        new TilingTypeConfig(
            tiling_name: "IH34",
            num_params: 0,
            num_aspects: 3,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_06,
            edge_orientations: eo_24,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_23,
            translation_coeffs: tc_09,
            aspect_coeffs: ac_26,
            colouring: c_05
        ),

        // IH35 is undefined
        new TilingTypeConfig(),

        // IH36
        new TilingTypeConfig(
            tiling_name: "IH36",
            num_params: 0,
            num_aspects: 3,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_06,
            edge_orientations: eo_25,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_23,
            translation_coeffs: tc_08,
            aspect_coeffs: ac_27,
            colouring: c_05
        ),

        // IH37
        new TilingTypeConfig(
            tiling_name: "IH37",
            num_params: 0,
            num_aspects: 3,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_15,
            edge_orientations: eo_26,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_23,
            translation_coeffs: tc_08,
            aspect_coeffs: ac_28,
            colouring: c_05
        ),

        // IH38
        new TilingTypeConfig(
            tiling_name: "IH38",
            num_params: 0,
            num_aspects: 6,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_10,
            edge_orientations: eo_27,
            edge_shape_ids: esi_21,
            default_params: dp_08,
            vertex_coeffs: tvc_24,
            translation_coeffs: tc_27,
            aspect_coeffs: ac_29,
            colouring: c_15
        ),

        // IH39
        new TilingTypeConfig(
            tiling_name: "IH39",
            num_params: 0,
            num_aspects: 6,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_25,
            edge_orientations: eo_27,
            edge_shape_ids: esi_21,
            default_params: dp_08,
            vertex_coeffs: tvc_24,
            translation_coeffs: tc_28,
            aspect_coeffs: ac_30,
            colouring: c_16
        ),

        // IH40
        new TilingTypeConfig(
            tiling_name: "IH40",
            num_params: 0,
            num_aspects: 6,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_24,
            edge_orientations: eo_28,
            edge_shape_ids: esi_21,
            default_params: dp_08,
            vertex_coeffs: tvc_24,
            translation_coeffs: tc_29,
            aspect_coeffs: ac_31,
            colouring: c_17
        ),

        // IH41
        new TilingTypeConfig(
            tiling_name: "IH41",
            num_params: 2,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_23,
            edge_orientations: eo_22,
            edge_shape_ids: esi_22,
            default_params: dp_21,
            vertex_coeffs: tvc_25,
            translation_coeffs: tc_30,
            aspect_coeffs: ac_09,
            colouring: c_18
        ),

        // IH42
        new TilingTypeConfig(
            tiling_name: "IH42",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_22,
            edge_orientations: eo_29,
            edge_shape_ids: esi_23,
            default_params: dp_21,
            vertex_coeffs: tvc_25,
            translation_coeffs: tc_31,
            aspect_coeffs: ac_32,
            colouring: c_19
        ),

        // IH43
        new TilingTypeConfig(
            tiling_name: "IH43",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_23,
            edge_orientations: eo_30,
            edge_shape_ids: esi_22,
            default_params: dp_21,
            vertex_coeffs: tvc_25,
            translation_coeffs: tc_31,
            aspect_coeffs: ac_33,
            colouring: c_19
        ),

        // IH44
        new TilingTypeConfig(
            tiling_name: "IH44",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_23,
            edge_orientations: eo_31,
            edge_shape_ids: esi_24,
            default_params: dp_22,
            vertex_coeffs: tvc_26,
            translation_coeffs: tc_32,
            aspect_coeffs: ac_34,
            colouring: c_20
        ),

        // IH45
        new TilingTypeConfig(
            tiling_name: "IH45",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_22,
            edge_orientations: eo_32,
            edge_shape_ids: esi_23,
            default_params: dp_23,
            vertex_coeffs: tvc_27,
            translation_coeffs: tc_33,
            aspect_coeffs: ac_35,
            colouring: c_20
        ),

        // IH46
        new TilingTypeConfig(
            tiling_name: "IH46",
            num_params: 4,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 4,
            edge_shapes: es_26,
            edge_orientations: eo_33,
            edge_shape_ids: esi_25,
            default_params: dp_24,
            vertex_coeffs: tvc_28,
            translation_coeffs: tc_34,
            aspect_coeffs: ac_36,
            colouring: c_20
        ),

        // IH47
        new TilingTypeConfig(
            tiling_name: "IH47",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_27,
            edge_orientations: eo_29,
            edge_shape_ids: esi_23,
            default_params: dp_21,
            vertex_coeffs: tvc_25,
            translation_coeffs: tc_35,
            aspect_coeffs: ac_37,
            colouring: c_19
        ),

        // IH48 is undefined
        new TilingTypeConfig(),

        // IH49
        new TilingTypeConfig(
            tiling_name: "IH49",
            num_params: 3,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 4,
            edge_shapes: es_28,
            edge_orientations: eo_33,
            edge_shape_ids: esi_25,
            default_params: dp_25,
            vertex_coeffs: tvc_29,
            translation_coeffs: tc_36,
            aspect_coeffs: ac_38,
            colouring: c_21
        ),

        // IH50
        new TilingTypeConfig(
            tiling_name: "IH50",
            num_params: 2,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_29,
            edge_orientations: eo_29,
            edge_shape_ids: esi_23,
            default_params: dp_21,
            vertex_coeffs: tvc_25,
            translation_coeffs: tc_37,
            aspect_coeffs: ac_39,
            colouring: c_22
        ),

        // IH51
        new TilingTypeConfig(
            tiling_name: "IH51",
            num_params: 3,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_27,
            edge_orientations: eo_32,
            edge_shape_ids: esi_23,
            default_params: dp_26,
            vertex_coeffs: tvc_30,
            translation_coeffs: tc_38,
            aspect_coeffs: ac_40,
            colouring: c_21
        ),

        // IH52
        new TilingTypeConfig(
            tiling_name: "IH52",
            num_params: 1,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_23,
            edge_orientations: eo_34,
            edge_shape_ids: esi_22,
            default_params: dp_20,
            vertex_coeffs: tvc_31,
            translation_coeffs: tc_39,
            aspect_coeffs: ac_41,
            colouring: c_23
        ),

        // IH53
        new TilingTypeConfig(
            tiling_name: "IH53",
            num_params: 3,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_27,
            edge_orientations: eo_35,
            edge_shape_ids: esi_26,
            default_params: dp_27,
            vertex_coeffs: tvc_32,
            translation_coeffs: tc_40,
            aspect_coeffs: ac_42,
            colouring: c_21
        ),

        // IH54
        new TilingTypeConfig(
            tiling_name: "IH54",
            num_params: 2,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 4,
            edge_shapes: es_30,
            edge_orientations: eo_33,
            edge_shape_ids: esi_25,
            default_params: dp_28,
            vertex_coeffs: tvc_33,
            translation_coeffs: tc_41,
            aspect_coeffs: ac_43,
            colouring: c_22
        ),

        // IH55
        new TilingTypeConfig(
            tiling_name: "IH55",
            num_params: 0,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_23,
            edge_orientations: eo_24,
            edge_shape_ids: esi_24,
            default_params: dp_08,
            vertex_coeffs: tvc_34,
            translation_coeffs: tc_42,
            aspect_coeffs: ac_44,
            colouring: c_24
        ),

        // IH56
        new TilingTypeConfig(
            tiling_name: "IH56",
            num_params: 1,
            num_aspects: 8,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_22,
            edge_orientations: eo_36,
            edge_shape_ids: esi_26,
            default_params: dp_29,
            vertex_coeffs: tvc_35,
            translation_coeffs: tc_43,
            aspect_coeffs: ac_45,
            colouring: c_25
        ),

        // IH57
        new TilingTypeConfig(
            tiling_name: "IH57",
            num_params: 2,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_31,
            edge_orientations: eo_33,
            edge_shape_ids: esi_22,
            default_params: dp_21,
            vertex_coeffs: tvc_25,
            translation_coeffs: tc_30,
            aspect_coeffs: ac_09,
            colouring: c_18
        ),

        // IH58
        new TilingTypeConfig(
            tiling_name: "IH58",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_32,
            edge_orientations: eo_33,
            edge_shape_ids: esi_22,
            default_params: dp_21,
            vertex_coeffs: tvc_25,
            translation_coeffs: tc_31,
            aspect_coeffs: ac_32,
            colouring: c_19
        ),

        // IH59
        new TilingTypeConfig(
            tiling_name: "IH59",
            num_params: 1,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_06,
            edge_orientations: eo_31,
            edge_shape_ids: esi_20,
            default_params: dp_30,
            vertex_coeffs: tvc_36,
            translation_coeffs: tc_44,
            aspect_coeffs: ac_46,
            colouring: c_20
        ),

        // IH60 is undefined
        new TilingTypeConfig(),

        // IH61
        new TilingTypeConfig(
            tiling_name: "IH61",
            num_params: 0,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_06,
            edge_orientations: eo_24,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_34,
            translation_coeffs: tc_45,
            aspect_coeffs: ac_47,
            colouring: c_20
        ),

        // IH62
        new TilingTypeConfig(
            tiling_name: "IH62",
            num_params: 0,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_07,
            edge_orientations: eo_33,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_34,
            translation_coeffs: tc_46,
            aspect_coeffs: ac_08,
            colouring: c_18
        ),

        // IH63 is undefined
        new TilingTypeConfig(),

        // IH64
        new TilingTypeConfig(
            tiling_name: "IH64",
            num_params: 1,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_33,
            edge_orientations: eo_37,
            edge_shape_ids: esi_22,
            default_params: dp_20,
            vertex_coeffs: tvc_31,
            translation_coeffs: tc_47,
            aspect_coeffs: ac_48,
            colouring: c_18
        ),

        // IH65 is undefined
        new TilingTypeConfig(),

        // IH66
        new TilingTypeConfig(
            tiling_name: "IH66",
            num_params: 1,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_34,
            edge_orientations: eo_37,
            edge_shape_ids: esi_22,
            default_params: dp_20,
            vertex_coeffs: tvc_31,
            translation_coeffs: tc_48,
            aspect_coeffs: ac_49,
            colouring: c_19
        ),

        // IH67
        new TilingTypeConfig(
            tiling_name: "IH67",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 3,
            edge_shapes: es_21,
            edge_orientations: eo_38,
            edge_shape_ids: esi_23,
            default_params: dp_23,
            vertex_coeffs: tvc_27,
            translation_coeffs: tc_49,
            aspect_coeffs: ac_50,
            colouring: c_20
        ),

        // IH68
        new TilingTypeConfig(
            tiling_name: "IH68",
            num_params: 1,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_06,
            edge_orientations: eo_39,
            edge_shape_ids: esi_20,
            default_params: dp_30,
            vertex_coeffs: tvc_36,
            translation_coeffs: tc_50,
            aspect_coeffs: ac_48,
            colouring: c_18
        ),

        // IH69
        new TilingTypeConfig(
            tiling_name: "IH69",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_31,
            edge_orientations: eo_26,
            edge_shape_ids: esi_24,
            default_params: dp_22,
            vertex_coeffs: tvc_26,
            translation_coeffs: tc_32,
            aspect_coeffs: ac_51,
            colouring: c_20
        ),

        // IH70 is undefined
        new TilingTypeConfig(),

        // IH71
        new TilingTypeConfig(
            tiling_name: "IH71",
            num_params: 0,
            num_aspects: 4,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_06,
            edge_orientations: eo_40,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_34,
            translation_coeffs: tc_42,
            aspect_coeffs: ac_52,
            colouring: c_24
        ),

        // IH72
        new TilingTypeConfig(
            tiling_name: "IH72",
            num_params: 1,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 2,
            edge_shapes: es_24,
            edge_orientations: eo_33,
            edge_shape_ids: esi_22,
            default_params: dp_20,
            vertex_coeffs: tvc_31,
            translation_coeffs: tc_47,
            aspect_coeffs: ac_48,
            colouring: c_18
        ),

        // IH73
        new TilingTypeConfig(
            tiling_name: "IH73",
            num_params: 0,
            num_aspects: 2,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_14,
            edge_orientations: eo_24,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_34,
            translation_coeffs: tc_45,
            aspect_coeffs: ac_47,
            colouring: c_20
        ),

        // IH74
        new TilingTypeConfig(
            tiling_name: "IH74",
            num_params: 1,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_07,
            edge_orientations: eo_26,
            edge_shape_ids: esi_20,
            default_params: dp_30,
            vertex_coeffs: tvc_36,
            translation_coeffs: tc_50,
            aspect_coeffs: ac_48,
            colouring: c_18
        ),

        // IH75 is undefined
        new TilingTypeConfig(),

        // IH76
        new TilingTypeConfig(
            tiling_name: "IH76",
            num_params: 0,
            num_aspects: 1,
            num_vertices: 4,
            num_edge_shapes: 1,
            edge_shapes: es_15,
            edge_orientations: eo_33,
            edge_shape_ids: esi_20,
            default_params: dp_08,
            vertex_coeffs: tvc_34,
            translation_coeffs: tc_46,
            aspect_coeffs: ac_08,
            colouring: c_18
        ),

        // IH77
        new TilingTypeConfig(
            tiling_name: "IH77",
            num_params: 0,
            num_aspects: 12,
            num_vertices: 3,
            num_edge_shapes: 3,
            edge_shapes: es_35,
            edge_orientations: eo_41,
            edge_shape_ids: esi_27,
            default_params: dp_08,
            vertex_coeffs: tvc_37,
            translation_coeffs: tc_51,
            aspect_coeffs: ac_53,
            colouring: c_26
        ),

        // IH78
        new TilingTypeConfig(
            tiling_name: "IH78",
            num_params: 1,
            num_aspects: 4,
            num_vertices: 3,
            num_edge_shapes: 3,
            edge_shapes: es_36,
            edge_orientations: eo_41,
            edge_shape_ids: esi_27,
            default_params: dp_20,
            vertex_coeffs: tvc_38,
            translation_coeffs: tc_52,
            aspect_coeffs: ac_54,
            colouring: c_22
        ),

        // IH79
        new TilingTypeConfig(
            tiling_name: "IH79",
            num_params: 0,
            num_aspects: 4,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_25,
            edge_orientations: eo_27,
            edge_shape_ids: esi_21,
            default_params: dp_08,
            vertex_coeffs: tvc_39,
            translation_coeffs: tc_53,
            aspect_coeffs: ac_55,
            colouring: c_27
        ),

        // IH80 is undefined
        new TilingTypeConfig(),

        // IH81
        new TilingTypeConfig(
            tiling_name: "IH81",
            num_params: 0,
            num_aspects: 8,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_10,
            edge_orientations: eo_27,
            edge_shape_ids: esi_21,
            default_params: dp_08,
            vertex_coeffs: tvc_39,
            translation_coeffs: tc_54,
            aspect_coeffs: ac_56,
            colouring: c_25
        ),

        // IH82
        new TilingTypeConfig(
            tiling_name: "IH82",
            num_params: 0,
            num_aspects: 4,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_24,
            edge_orientations: eo_28,
            edge_shape_ids: esi_21,
            default_params: dp_08,
            vertex_coeffs: tvc_39,
            translation_coeffs: tc_55,
            aspect_coeffs: ac_57,
            colouring: c_27
        ),

        // IH83
        new TilingTypeConfig(
            tiling_name: "IH83",
            num_params: 1,
            num_aspects: 2,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_10,
            edge_orientations: eo_42,
            edge_shape_ids: esi_28,
            default_params: dp_31,
            vertex_coeffs: tvc_40,
            translation_coeffs: tc_56,
            aspect_coeffs: ac_58,
            colouring: c_20
        ),

        // IH84
        new TilingTypeConfig(
            tiling_name: "IH84",
            num_params: 2,
            num_aspects: 2,
            num_vertices: 3,
            num_edge_shapes: 3,
            edge_shapes: es_04,
            edge_orientations: eo_41,
            edge_shape_ids: esi_27,
            default_params: dp_32,
            vertex_coeffs: tvc_41,
            translation_coeffs: tc_57,
            aspect_coeffs: ac_59,
            colouring: c_20
        ),

        // IH85
        new TilingTypeConfig(
            tiling_name: "IH85",
            num_params: 2,
            num_aspects: 4,
            num_vertices: 3,
            num_edge_shapes: 3,
            edge_shapes: es_37,
            edge_orientations: eo_41,
            edge_shape_ids: esi_27,
            default_params: dp_32,
            vertex_coeffs: tvc_41,
            translation_coeffs: tc_58,
            aspect_coeffs: ac_60,
            colouring: c_21
        ),

        // IH86
        new TilingTypeConfig(
            tiling_name: "IH86",
            num_params: 1,
            num_aspects: 4,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_25,
            edge_orientations: eo_42,
            edge_shape_ids: esi_28,
            default_params: dp_31,
            vertex_coeffs: tvc_40,
            translation_coeffs: tc_59,
            aspect_coeffs: ac_61,
            colouring: c_21
        ),

        // IH87 is undefined
        new TilingTypeConfig(),

        // IH88
        new TilingTypeConfig(
            tiling_name: "IH88",
            num_params: 0,
            num_aspects: 6,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_25,
            edge_orientations: eo_43,
            edge_shape_ids: esi_28,
            default_params: dp_08,
            vertex_coeffs: tvc_42,
            translation_coeffs: tc_60,
            aspect_coeffs: ac_62,
            colouring: c_28
        ),

        // IH89 is undefined
        new TilingTypeConfig(),

        // IH90
        new TilingTypeConfig(
            tiling_name: "IH90",
            num_params: 0,
            num_aspects: 2,
            num_vertices: 3,
            num_edge_shapes: 1,
            edge_shapes: es_07,
            edge_orientations: eo_41,
            edge_shape_ids: esi_29,
            default_params: dp_08,
            vertex_coeffs: tvc_42,
            translation_coeffs: tc_09,
            aspect_coeffs: ac_63,
            colouring: c_20
        ),

        // IH91
        new TilingTypeConfig(
            tiling_name: "IH91",
            num_params: 1,
            num_aspects: 2,
            num_vertices: 3,
            num_edge_shapes: 2,
            edge_shapes: es_32,
            edge_orientations: eo_44,
            edge_shape_ids: esi_28,
            default_params: dp_31,
            vertex_coeffs: tvc_40,
            translation_coeffs: tc_61,
            aspect_coeffs: ac_64,
            colouring: c_20
        ),

        // IH92 is undefined
        new TilingTypeConfig(),

        // IH93
        new TilingTypeConfig(
            tiling_name: "IH93",
            num_params: 0,
            num_aspects: 2,
            num_vertices: 3,
            num_edge_shapes: 1,
            edge_shapes: es_15,
            edge_orientations: eo_41,
            edge_shape_ids: esi_29,
            default_params: dp_08,
            vertex_coeffs: tvc_42,
            translation_coeffs: tc_09,
            aspect_coeffs: ac_63,
            colouring: c_20
        )
    };
}



public class IsohedralTiling {
    
    private TilingTypeConfig ttd;
    private List<double> parameters;
    public List<Vector2> verts;
    private List<float[]> edges { get; set; }
    private List<bool> reversals { get; set; }


    int numTypes = 81;

    public static readonly int[] tilingTypes = new int[]
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        11, 12, 13, 14, 15, 16, 17, 18, 20,
        21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
        31, 32, 33, 34, 36, 37, 38, 39, 40,
        41, 42, 43, 44, 45, 46, 47, 49, 50,
        51, 52, 53, 54, 55, 56, 57, 58, 59,
        61, 62, 64, 66, 67, 68, 69,
        71, 72, 73, 74, 76, 77, 78, 79,
        81, 82, 83, 84, 85, 86, 88,
        90, 91, 93
    };

    private float[][] M_orients =
    {
        new [] {1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f},   // IDENTITY
        new [] {-1.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f}, // ROT
        new [] {-1.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f}, // FLIP
        new [] {1.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f} // ROFL
    };

    private float[][] TSPI_U =
    {
        new[]{0.5f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f},
        new[]{-0.5f, 0.0f, 1.0f, 0.0f, 0.5f, 0.0f}
    };

    private float[][] TSPI_S =
    {
        new [] {0.5f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f},
        new [] {-0.5f, 0.0f, 1.0f, 0.0f, -0.5f, 0.0f}
    };

    private List<double[]> aspects;
    private Vector2 t1;
    private Vector2 t2;
    private int tiling_type;

    private Vector2 makePoint(double[] coeffs, int offs, double[] par)
    {
        var ret = Vector2.zero;

        for (int i = 0; i < par.Length; ++i)
        {
            ret.x += (float)(coeffs[offs + i] * par[i]);
            ret.y += (float)(coeffs[offs + par.Length + i] * par[i]);
        }

        return ret;
    }

    private double[] makeMatrix(double[] coeffs, int offs, double[] par)
    {
        var ret = new double[6];

        int counter = 0;
        
        for (int row = 0; row < 2; ++row)
        {
            for (int col = 0; col < 3; ++col)
            {
                double val = 0.0;
                for (int idx = 0; idx < par.Length; ++idx)
                {
                    val += coeffs[offs + idx] * par[idx];
                }

                ret[counter] = val;
                counter++;
                offs += par.Length;
            }
        }

        return ret;
    }
    
    public static Vector2 Multiply(float[] A, Vector2 B)
    {
        // Matrix * Point
        return new Vector2(
            x: A[0] * B.x + A[1] * B.y + A[2],
            y: A[3] * B.x + A[4] * B.y + A[5]
        );
    }

    public static float[] Multiply(float[] A, float[] B)
    {
        // Matrix * Matrix
        return new float[]
        {
            A[0] * B[0] + A[1] * B[3],
            A[0] * B[1] + A[1] * B[4],
            A[0] * B[2] + A[1] * B[5] + A[2],

            A[3] * B[0] + A[4] * B[3],
            A[3] * B[1] + A[4] * B[4],
            A[3] * B[2] + A[4] * B[5] + A[5]
        };
    }

    public float[] matchSeg(Vector2 p, Vector2 q)
    {
        return new []
        {
            q.x - p.x,
            p.y - q.y,
            p.x,
            q.y - p.y,
            q.x - p.x,
            p.y
        };
    }

    public IsohedralTiling(int tp)
    {
        Reset(tp);
    }
    
    public void Reset(int tp)
    {
        if (!tilingTypes.Contains(tp))
        {
            Debug.LogWarning("Invalid tiling type");
        }
        tiling_type = tp;
        ttd = IsohedralTilingHelpers.tiling_types[tp];
        parameters = new List<double>(ttd.default_params);
        parameters.Add(1.0);
        Recompute();
    }
    
    public void Recompute()
    {
        var ntv = numVertices();
        var np = numParameters();
        var na = numAspects();
        
        // Recompute tiling vertex locations.
        verts = new List<Vector2>();
        for (int idx = 0; idx < ntv; ++idx)
        {
            verts.Add(
                makePoint(
                    ttd.vertex_coeffs,
                    idx * (2 * (np + 1)),
                    parameters.ToArray()
                )
            );
        }
        
        // Recompute edge transforms and reversals from orientation information.
        reversals = new List<bool>();
        edges = new List<float[]>();
        for (int idx = 0; idx < ntv; ++idx) {
            var fl = ttd.edge_orientations[2 * idx];
            var ro = ttd.edge_orientations[2 * idx + 1];
            reversals.Add(fl != ro);
            float[] orientation = new float[] { };
            if (ro==false && fl==false)
            {
                orientation = M_orients[0];
            }
            else if (ro==true && fl==false)
            {
                orientation = M_orients[1];
            }
            else if (ro==false && fl==true)
            {
                orientation = M_orients[2];
            }
            else if (ro==true && fl==true)
            {
                orientation = M_orients[3];
            }
            
            edges.Add(
                Multiply(
                    matchSeg(verts[idx], verts[(idx + 1) % ntv]),
                    orientation
                )
            );
        }
        
        // Recompute aspect xforms.
        var param = parameters.ToArray();
        aspects = new List<double[]>();
        for (int idx = 0; idx < na; ++idx)
        {
            aspects.Add(
                makeMatrix(ttd.aspect_coeffs, 6 * (np + 1) * idx, param)
            );
        }

        // Recompute translation vectors.
        t1 = makePoint(ttd.translation_coeffs, 0, param);
        t2 = makePoint(ttd.translation_coeffs, 2 * (np + 1), param);
    }

    public int numParameters()
    {
        return ttd.num_params;
    }
    
    public string tilingName()
    {
        return ttd.tiling_name;
    }

    public void setParameters(List<double> arr)
    {
        if (arr.Count == parameters.Count - 1)
        {
            parameters = new List<double>(arr);
            parameters.Add(1.0);
            Recompute();
        }
    }

    public List<double> getParameters()
    {
        return parameters.Take(parameters.Count - 1).ToList();
    }

    public int numEdgeShapes()
    {
        return ttd.num_edge_shapes;
    }

    public EdgeShape getEdgeShape(int idx)
    {
        return ttd.edge_shapes[idx];
    }

    public struct Thing
    {
        public float[] T;
        public int id;
        public EdgeShape shape;
        public bool rev;
        public bool second;
        public float t1;
        public float t2;
        public int aspect;

        public Thing(
            float[] T,
            int id=0,
            EdgeShape shape=EdgeShape.J,
            bool rev=false,
            bool second=false,
            float t1=0,
            float t2=0,
            int aspect=0)
        {
            this.T = T;
            this.t1 = t1;
            this.t2 = t2;
            this.id = id;
            this.shape = shape;
            this.rev = rev;
            this.second = second;
            this.aspect = aspect;
        }
    }

    public IEnumerable<Thing> shape()
    {
        
        for (int idx = 0; idx < numVertices(); ++idx)
        {
            var an_id = ttd.edge_shape_ids[idx];

            yield return new Thing(
                T: edges[idx],
                id: an_id,
                shape: ttd.edge_shapes[an_id],
                rev: reversals[idx]
            );
        }
    }

    public IEnumerable<Thing> parts()
    {
        for (int idx = 0; idx < numVertices(); ++idx)
        {
            var an_id = ttd.edge_shape_ids[idx];
            var shp = ttd.edge_shapes[an_id];

            if (shp == EdgeShape.J || shp == EdgeShape.I)
            {
                yield return new Thing(
                    T: edges[idx],
                    id: an_id,
                    shape: shp,
                    rev: reversals[idx],
                    second: false
                );
            } else {
                (int, int) indices = reversals[idx] ? (1, 0) : (0, 1);
                var Ms = (shp == EdgeShape.U) ? TSPI_U : TSPI_S;

                yield return new Thing(
                    T: Multiply(edges[idx], Ms[indices.Item1]),
                    id: an_id,
                    shape: shp,
                    rev: false,
                    second: false
                );

                yield return new Thing(
                    T: Multiply(edges[idx], Ms[indices.Item2]),
                    id: an_id,
                    shape: shp,
                    rev: true,
                    second: true
                );
            }
        }
    }

    public int numVertices()
    {
        return ttd.num_vertices;
    }

    // public Vector2 getVertex(int idx) {
    //     return {...verts[idx]};
    // }

    // public List<Vector2> vertices() {
    //     return verts.Select(v => ({...v}));
    // }

    public int numAspects()
    {
        return ttd.num_aspects;
    }

    // public void getAspectTransform(idx) {
    //     return [...aspects[idx]];
    // }

    public IEnumerable<IEnumerable<IEnumerable<Thing>>> fillRegionBounds(float xmin, float ymin, float xmax, float ymax)
    {
        return fillRegionQuad(
            new Vector2(xmin, ymin),
            new Vector2(xmax, ymin),
            new Vector2(xmax, ymax),
            new Vector2(xmin, ymax)
        );
    }

    public IEnumerable<IEnumerable<IEnumerable<Thing>>> fillRegionQuad(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
    {
        
        Vector2 bc(double[] M, Vector2 p)
        {
            return new Vector2(
                (float)(M[0] * p.x + M[1] * p.y),
                (float)(M[2] * p.x + M[3] * p.y)
            );
        };

        Vector2 sampleAtHeight(Vector2 P, Vector2 Q, float y)
        {
            float t = (y - P.y) / (Q.y - P.y);
            return new Vector2((1.0f - t) * P.x + t * Q.x, y);
        }

        IEnumerable<Thing> doFill(Vector2 AA, Vector2 BB, Vector2 CC, Vector2 DD, bool do_top)
        {
            float x1 = AA.x;
            float dx1 = (DD.x - AA.x) / (DD.y - AA.y);
            float x2 = BB.x;
            float dx2 = (CC.x - BB.x) / (CC.y - BB.y);
            float ymin = AA.y;
            float ymax = CC.y;

            if (do_top)
            {
                ymax = ymax + 1.0f;
            }

            double y = Math.Floor(ymin);
            while (y < ymax)
            {
                float yi = Mathf.Floor((float)y);
                var xx = Math.Floor(x1);
                while (xx < x2 + 1e-7)
                {
                    
                    float xi = Mathf.Floor((float)xx);

                    for (int asp = 0; asp < ttd.num_aspects; ++asp)
                    {
                        double[] M = new double[aspects[asp].Length];
                        aspects[asp].CopyTo(M, 0);
                        M[2] += xi * t1.x + yi * t2.x;
                        M[5] += xi * t1.y + yi * t2.y;

                        yield return new Thing(
                            T: M.Select(x=>(float)x).ToArray(),
                            t1: xi,
                            t2: yi,
                            aspect: asp
                        );
                    }

                    xx += 1.0;
                }
                x1 += dx1;
                x2 += dx2;
                y += 1.0;
            }
        }

        IEnumerable<IEnumerable<Thing>> fillFixX(Vector2 AA, Vector2 BB, Vector2 CC, Vector2 DD, bool do_top)
        {
            if (AA.x > BB.x)
            {
                yield return doFill(BB, AA, DD, CC, do_top);
            }
            else
            {
                yield return doFill(AA, BB, CC, DD, do_top);
            }
        }

        IEnumerable<IEnumerable<Thing>> fillFixY(Vector2 AA, Vector2 BB, Vector2 CC, Vector2 DD, bool do_top)
        {
            if (AA.y > CC.y)
            {
                yield return doFill(CC, DD, AA, BB, do_top);
            }
            else
            {
                yield return doFill(AA, BB, CC, DD, do_top);
            }
        }

        var det = 1.0 / (t1.x * t2.y - t2.x * t1.y);
        var Mbc = new[]{t2.y * det, -t2.x * det, -t1.y * det, t1.x * det};
        var pts = new[]{bc(Mbc, A), bc(Mbc, B), bc(Mbc, C), bc(Mbc, D)};

        if (det < 0.0)
        {
            var tmp = pts[1];
            pts[1] = pts[3];
            pts[3] = tmp;
        }

        if (Math.Abs(pts[0].y - pts[1].y) < 1e-7)
        {
            yield return fillFixY(pts[0], pts[1], pts[2], pts[3], true);
        }
        else if (Math.Abs(pts[1].y - pts[2].y) < 1e-7)
        {
            yield return fillFixY(pts[1], pts[2], pts[3], pts[0], true);
        }
        else
        {
            int lowest = 0;
            for (int idx = 1; idx < 4; ++idx) {
                if (pts[idx].y < pts[lowest].y) {
                    lowest = idx;
                }
            }

            var bottom = pts[lowest];
            var left = pts[(lowest + 1) % 4];
            var top = pts[(lowest + 2) % 4];
            var right = pts[(lowest + 3) % 4];

            if (left.x > right.x)
            {
                var tmp = left;
                left = right;
                right = tmp;
            }

            if (left.y < right.y)
            {
                var r1 = sampleAtHeight(bottom, right, left.y);
                var l2 = sampleAtHeight(left, top, right.y);
                yield return fillFixX(bottom, bottom, r1, left, false);
                yield return fillFixX(left, r1, right, l2, false);
                yield return fillFixX(l2, right, top, top, true);
            }
            else
            {
                var l1 = sampleAtHeight(bottom, left, right.y);
                var r2 = sampleAtHeight(right, top, left.y);
                yield return fillFixX(bottom, bottom, right, l1, false);
                yield return fillFixX(l1, right, r2, left, false);
                yield return fillFixX(left, r2, top, top, true);
            }
        }
    }

    public int getColour(int a, int b, int asp) {
        var clrg = ttd.colouring;
        var nc = clrg[18];

        var mt1 = a % nc;
        if (mt1 < 0) {
            mt1 += nc;
        }
        var mt2 = b % nc;
        if (mt2 < 0) {
            mt2 += nc;
        }
        var col = clrg[asp];

        for (int idx = 0; idx < mt1; ++idx) {
            col = clrg[12 + col];
        }
        for (int idx = 0; idx < mt2; ++idx) {
            col = clrg[15 + col];
        }

        return col;
    }

}



