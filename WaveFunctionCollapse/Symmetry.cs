using System;
using System.ComponentModel;
using static Symmetry;

public enum Symmetry
{
    F,
    L,
    I,
    X,
    T,
    [Description("\\")] Slash,
}

public static class SymmetryHelper
{
    
    public static bool IsRightEdgeVerticalSymmetry(this Symmetry symmetry, int rotate = 0)
        => (symmetry, rotate) switch
        {
            (X, _) => true,
            (L, _) => true,
            (I, _) => true,
            (Slash, _) => false,
            _ => false,
        };

    /// <summary>
    /// Get rotate value after vertical flip
    /// </summary>
    /// <param name="symmetry"></param>
    /// <param name="rotate"></param>
    /// <returns>Rotate value, -1 means no rotate or self</returns>
    public static int GetRotateByVerticalFlip(this Symmetry symmetry, int rotate = 0)
        => (symmetry, rotate) switch
        {
            (L, 0) => 3,
            (L, 3) => 0,
            (L, 1) => 2,
            (L, 2) => 1,
            (T, 0) => 2,
            (T, 2) => 0,
            // (T, 1) => 1,
            // (T, 3) => 3,
            (T, _) => -1,
            (I, _) => -1,
            (X, _) => -1,
            (F, _) => -1,
            (Slash, 0) => 1, // 3
            (Slash, 1) => 0, // 3
            _ => -1,
        };

    public static bool IsHorizontalSymmetry(this Symmetry symmetry, int rotate = 0)
        => (symmetry, rotate) switch
        {
            (I, _) => true,
            (T, 0 or 2) => true,
            _ => false,
        };

    public static int GetRotateByHorizontalFlip(this Symmetry symmetry, int rotate = 0)
        => (symmetry, rotate) switch
        {
            (L, 0) => 1,
            (L, 1) => 0,
            (L, 2) => 3,
            (L, 3) => 2,
            _ => -1,
        };

    public static int GetRotateWithSameRightEdge(this Symmetry symmetry, int rotate = 0)
        => (symmetry, rotate) switch
        {
            (L, 0) => 3,
            (L, 3) => 0,
            (L, 2) => 1,
            (L, 1) => 2,
            (T, 0) => 2,
            (T, 2) => 0,
            _ => -1,
        };

    /// <summary>
    /// 获取旋转以后，是否真正的旋转了
    /// </summary>
    /// <param name="symmetry"></param>
    /// <param name="rotate"></param>
    /// <returns></returns>
    public static int GetOriginalRotate(this Symmetry symmetry, int rotate = 0)
        => (symmetry, rotate) switch
        {
            (I or Slash, {} i) when i >= 2 => i - 2,
            (X, _) => 0,
            // (L or F, _) => rotate,
            _ => rotate,
        };

    /// <summary>
    /// 展示最多种可能的旋转
    /// </summary>
    /// <param name="symmetry"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static long Encoding(this Symmetry symmetry)
        => symmetry switch
        {
            F or L or T => 0b1111,
            I or Slash => 0b11,
            X => 0b1,
            _ => throw new ArgumentOutOfRangeException(nameof(symmetry), symmetry, null)
        };
    
    public static int RotationCount(this Symmetry symmetry)
        => symmetry switch
        {
            F or L or T => 4,
            I or Slash => 2,
            X => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(symmetry), symmetry, null)
        };
}
