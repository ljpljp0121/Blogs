## [二分查找](https://leetcode.cn/problems/binary-search/)
非常经典地一道题目，题目为有序数组且没有重复元素,这些都是二分法地前提条件。而其中最重要的就是边界划分，本题使用左闭右闭区间，所以两个边界都一定要有意义，所以right 初始化为nums.size()-1,如果初始化为nums.size()，那么就应该使用左闭右开区间了。  
然后就是既然使用了左闭右闭，所以left==right也有意义,所以有如下两点  
1. while (left <= right) 要使用 <= ，因为left == right是有意义的，所以使用 <=  
2. if (nums[middle] > target) right 要赋值为 middle - 1，因为当前这个nums[middle]一定不是target，那么接下来要查找的左区间结束下标位置就是 middle - 1 

```cpp    
class Solution {
public:
    int search(vector<int>& nums, int target) {
        int left = 0,right = nums.size()-1;
        while(left <= right)
        {
            int middle = left +(right-left)/2;
            if(nums[middle] >target)
            {
                right = middle -1;
            }
            else if(nums[middle] < target)
            {
                left = middle +1;
            }
            else
            {
                return middle;
            }
        }
        return -1;
    }
};
```