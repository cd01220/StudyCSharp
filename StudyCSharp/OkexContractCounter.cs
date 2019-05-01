using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyCSharp
{
    public class OkexContractCounter
    {
        /* 
        x0 = a / (y0 + b);  
        x1 = c / (y1 + d);
        y0 + y1 = e;

        求x0的值，使得 (f - x0)/f = (x1 - g)/g;

        其中 
        x0 = 资金划转后 contract0 的爆仓价
        x1 = 资金划转后 contract1 的爆仓价
        y0 = 资金划转后 contract0 的账户余额 (balance0)
        y1 = 资金划转后 contract1 的账户余额 (balance1)
        a = positionAmount0 * marginRatio0 - longAmount0 + shortAmount0;
        b = longPnl0 + shortPnl0 + longAmount0 / longPrice0 - shortAmount0 / shortPrice0 - frozenMargin0 * leverage0 * marginRatio0;
        c = positionVolume1 * marginRatio1 + longAmount1 - shortAmount1;
        d = longPnl1 + shortPnl1 - frozenMargin1 * marginRatio1 + longAmount1 / longPrice1 - shortAmount1 / shortPrice1;
        f = contract0 实时价格;
        g = contract1 实时价格;

        求解过程：
        (f - x0) / f = (x1 - g) / g
        => 1 - x0 / f = x1 / g - 1
        => x0 / f + x1 / g = 2                 
        => g * x0 + f * x1 = 2fg
        => ag / (y0 + b) + cf / (y1 + d) = 2fg
        => ag / (y0 + b) + cf / (e + d - y0) = 2fg
        => ag * (e + d - y0) + cf * (y0 + b) = 2fg * (y0 + b) * (e + d - y0)
        => age + agd - agy0 + cfy0 + cfb = 2fg * (ey0 + dy0 - y0**2 + be + bd - by0)
                                         = 2fgey0 + 2fgdy0 - 2fgy0**2 + 2fgbe + 2fgbd - 2fgby0

        => 2fgy0**2 + (cf - ag - 2fge - 2fgd + 2fgb)y0 + (age + agd + cfb - 2fgbe - 2fgbd) = 0
         */
        public static void TryCountTransferAmount()
        {
            const decimal denomination = 10m;
            const decimal balance = 3.6m;
            const decimal price = 160m;
            const decimal volume = 160m;

            /******************* contract0 *******************/
            decimal balance0 = balance;
            decimal longPrice0 = price;     // 多头结算基准价
            decimal shortPrice0 = price;    // 空头结算基准价 
            decimal longVolume0 = volume;   // 多头持仓量 (单位：张)
            decimal shortVolume0 = 0;     // 空头持仓量 (单位：张)            
            decimal longPnl0 = 0m;        // 多头已实现盈亏
            decimal shortPnl0 = 0m;       // 空头已实现盈亏
            decimal frozenMargin0 = 0m;   // 挂单冻结保证金

            (int Min, int Max, decimal MarginRatio)[] marginRatios = new[]
            {
                (0, 249, 0.0075m), (250, 4999, 0.015m), (5000, 24999, 0.02m), (25000, 37499, 0.03m),
                (37500, 49999, 0.04m), (50000, 62499, 0.05m), (62500, 74999, 0.06m)
            };

            const decimal leverage0 = 10m;  // 杠杆倍数
            decimal positionVolume0 = longVolume0 + shortVolume0;     // 多头持仓量 + 空头持仓量 (单位：张)            
            decimal longAmount0 = longVolume0 * denomination;      // 多头持仓金额
            decimal shortAmount0 = shortVolume0 * denomination;    // 空头持仓金额 
            decimal positionAmount0 = longAmount0 + shortAmount0;   // 多头+空头持仓总金额 
            decimal marginRatio0 = marginRatios.Single(x => positionVolume0 >= x.Min && positionVolume0 < x.Max).MarginRatio;

            /******************* contract1 *******************/
            decimal balance1 = balance;
            decimal longPrice1 = price;    // 多头结算基准价
            decimal shortPrice1 = price;   // 空头结算基准价 
            decimal longVolume1 = 0;       // 多头持仓量 (单位：张)
            decimal shortVolume1 = volume; // 空头持仓量 (单位：张)            
            decimal longPnl1 = 0m;         // 多头已实现盈亏
            decimal shortPnl1 = 0m;        // 空头已实现盈亏
            decimal frozenMargin1 = 0m;    // 挂单冻结保证金

            decimal marginRatio1 = 10 / 100.0m;
            decimal positionVolume1 = longVolume1 + shortVolume1;   // 多头持仓量 + 空头持仓量 (单位：张)            
            decimal longAmount1 = longVolume1 * denomination;      // 多头持仓金额
            decimal shortAmount1 = shortVolume1 * denomination;    // 空头持仓金额

            /******************* 求一元二次方程解 *******************/
            decimal a = positionAmount0 * marginRatio0 + longAmount0 - shortAmount0;
            decimal b = longPnl0 + shortPnl0 + longAmount0 / longPrice0 - shortAmount0 / shortPrice0 - frozenMargin0 * leverage0 * marginRatio0;
            decimal c = positionVolume1 * marginRatio1 + longAmount1 - shortAmount1;
            decimal d = longPnl1 + shortPnl1 + longAmount1 / longPrice1 - shortAmount1 / shortPrice1 - frozenMargin1 * marginRatio1;
            decimal e = balance0 + balance1;
            decimal f = price; // contract0 实时价格;
            decimal g = price; // contract1 实时价格;

            /* 资金划转前永续合约，交割合约的爆仓价 */
            Console.WriteLine($"contract0 Margin Call Price = {a / (balance0 + b)}");
            Console.WriteLine($"contract1 Margin Call Price = {c / (balance1 + d)}");

            // 2fgy0**2 + (cf - ag - 2fge - 2fgd + 2fgb)y0 + (age + agd + cfb - 2fgbe - 2fgbd) = 0
            // a0*x^2 + b0*x + c0 = 0; 
            double a0 = (double)(2 * f * g);
            double b0 = (double)(c * f - a * g - 2 * f * g * e - 2 * f * g * d + 2 * f * g * b);
            double c0 = (double)(a * g * e + a * g * d + c * f * b - 2 * f * g * b * e - 2 * f * g * b * d);
            double dt = b0 * b0 - 4 * a0 * c0; //Δ的值

            Debug.Assert(dt >= 0);
            if (dt == 0)
            {
                Console.WriteLine($"{-b0 / (2 * a0)}");
            }
            else
            {
                decimal contract0Balance = (decimal)Math.Abs((-b0 + Math.Sqrt(dt)) / (2 * a0));
                decimal contract1Balance = e - contract0Balance;
                decimal contract0Price = a / (contract0Balance + b);
                decimal contract1Price = c / (contract1Balance + d);

                Console.WriteLine($"contract 0 balance = {contract0Balance}, contract 1 balance = {contract1Balance}");
                Console.WriteLine($"contract 0 price = {contract0Price}, contract 1 price = {contract1Price}");
                decimal safetyRange = Math.Abs(1 - contract0Price / ((contract1Price + contract0Price) / 2));
                Console.WriteLine($"price safety range = {Math.Floor(safetyRange * 100)}%");
            }
        }
    }
}
