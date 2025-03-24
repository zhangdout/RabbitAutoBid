import { Auction, PagedResult } from "@/types";
import { create } from "zustand";

type State = {
  auctions: Auction[];
  totalCount: number;
  pageCount: number;
};

type Actions = {
  setData: (data: PagedResult<Auction>) => void;
  setCurrentPrice: (auctionId: string, amount: number) => void;
};

const initialState: State = {
  auctions: [],
  pageCount: 0,
  totalCount: 0,
};

//<State & Actions> 就是传入给 create 的 泛型类型参数。
//我这个 Zustand store 里面会包含两部分字段：
// State：保存数据，比如拍卖列表、页数等
// Actions：操作数据的方法，比如 setData()、setCurrentPrice()

/*
箭头函数的语法中：
(param) => { 
  // 这是函数体，需要 return 才会有返回值
}

(param) => ({ 
  // 这是直接返回一个对象，不用 return
})
*/

//所以这一句的意思是定义一个函数，它接收 set 参数，直接返回一个对象 {}，这个对象就是你的 store 状态。

/*
Zustand 的 create() 方法就需要你传入一个“返回对象”的函数，这个对象就是你 store 的初始状态，包括：
数据：auctions, totalCount, pageCount
方法：setData(), setCurrentPrice()
Zustand 会保存这个对象，并返回一个自定义的 Hook（useAuctionStore）。
*/
export const useAuctionStore = create<State & Actions>((set) => ({
  ...initialState,

  setData: (data: PagedResult<Auction>) => {
    set(() => ({
      auctions: data.results,
      totalCount: data.totalCount,
      pageCount: data.pageCount,
    }));
  },

  setCurrentPrice: (auctionId: string, amount: number) => {
    set((state) => ({
      auctions: state.auctions.map((auction) =>
        auction.id === auctionId
          ? { ...auction, currentHighBid: amount }
          : auction
      ),
    }));
  },
}));
