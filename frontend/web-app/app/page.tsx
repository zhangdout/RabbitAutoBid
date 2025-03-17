import Image from "next/image";
import Listings from "./auctions/Listings";
import AuctionCard from "./auctions/AuctionCard";
//所有 app 目录下的组件默认是 Server Components，除非显式声明为 Client Components。
//它们在 服务器端渲染，然后将静态 HTML 发送到客户端。这些组件不会在浏览器端运行，但可以直接访问数据库、调用后端 API、读取文件系统等。
/*
📌 Server Components 的特点
✅ 默认行为：所有 app 目录下的组件默认是 Server Components，除非显式声明为 Client Components。
✅ 在服务器端执行，不会被发送到浏览器。
✅ 可以直接访问数据库、后端 API、环境变量（process.env），更安全。
✅ 减少客户端 JavaScript 体积，加快页面加载速度。
✅ 适用于静态内容和 SEO 友好的页面。
❌ 不能使用 React 的 Hooks（如 useState, useEffect），因为它们依赖浏览器执行。
❌ 不能使用事件处理（如 onClick），因为没有运行在客户端。
*/



export default async function Home() {
  return (
    <div>
      <Listings />
    </div>
  );
}
