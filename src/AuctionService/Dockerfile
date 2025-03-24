# docker build -f src/AuctionService/Dockerfile -t testing123 .   ---构建镜像
# 这里.代表当前目录 RabbitAutoBid/。是构建上下文，即COPY/ADD命令访问的目录，从这个目录拷贝东西。

# as build → 这是一个 多阶段构建（第一阶段只用于构建，最终不会出现在生产环境）
FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
# 在容器内创建 /app 目录，并进入该目录。等于执行cd /app
WORKDIR /app
# EXPOSE 80 告诉 Docker 这个容器的应用会监听 80 端口。
# 但 EXPOSE 80 本身不会开放端口！ 只是一个文档信息，提醒使用这个镜像的人该应用监听的是 80 端口。
# 如果想要让外部访问，必须在 docker run 里指定 -p 参数：docker run -p 8080:80 auction-service
# 8080:80 → 把宿主机的 8080 端口映射到容器的 80 端口。这样，你就可以在 浏览器访问 http://localhost:8080 来访问容器内的应用
EXPOSE 80

# copy all .csproj files and restore as distinct layers.  Use the same COPY
# for every dockerfile in the project to take advantage of Docker caching
# 在 .NET 项目中，.csproj 是项目文件，里面包含了所有的 NuGet 依赖（类似于 package.json 或 requirements.txt）。
# Docker 构建是分层的（Layers）
# 在 Dockerfile 里，每一条命令（RUN、COPY、CMD）都会创建一个新的 缓存层（Layer）。

# Docker 构建时的核心规则：

# 如果某一层没有改动，Docker 就会使用缓存，不会重新执行。
# 如果某一层改动了，后续所有的层都会重新执行，不能使用缓存。

# dotnet restore 会下载所有 NuGet 依赖，这通常需要 几十秒甚至几分钟 ⏳。
# 如果 .csproj 没有改动，没必要每次都重新下载（浪费时间）。

# Docker 缓存的实际流程
# 第一次构建
#Step 1: COPY .csproj → ✅ 没有缓存，执行
#Step 2: RUN dotnet restore → ✅ 没有缓存，下载 NuGet 依赖
#Step 3: COPY 代码文件 → ✅ 没有缓存，执行
#Step 4: RUN dotnet publish → ✅ 没有缓存，执行。所有步骤都需要执行，NuGet 依赖会被下载。

#第二次构建（如果 .csproj 没有变化）
#Step 1: COPY .csproj → ✅ 使用缓存
#Step 2: RUN dotnet restore → ✅ 使用缓存（不重复下载 NuGet）
#Step 3: COPY 代码文件 → ✅ 代码变了，重新执行
#Step 4: RUN dotnet publish → ✅ 代码变了，重新执行。NuGet 依赖不会重新下载，直接使用缓存，构建速度大幅提升！

#如果只有 .csproj 改动了
#Step 1: COPY .csproj → ❌ 发生变化，Docker 重新执行
#Step 2: RUN dotnet restore → ❌ 重新下载 NuGet 依赖
#Step 3: COPY 代码文件 → ✅ 代码没变，使用缓存
#Step 4: RUN dotnet publish → ✅ 代码没变，使用缓存。只有 dotnet restore 重新执行，后续步骤仍然可以利用缓存，不用重新构建整个应用。

COPY RabbitAutoBid.sln RabbitAutoBid.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/BiddingService/BiddingService.csproj src/BiddingService/BiddingService.csproj
COPY src/NotificationService/NotificationService.csproj src/NotificationService/NotificationService.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY tests/AuctionService.UnitTests/AuctionService.UnitTests.csproj tests/AuctionService.UnitTests/AuctionService.UnitTests.csproj
COPY tests/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj tests/AuctionService.IntegrationTests/AuctionService.IntegrationTests.csproj


# Restore package deps
RUN dotnet restore RabbitAutoBid.sln

# Copy the app folders over
# 复制完整的代码。这样 dotnet build 可以编译所有代码
COPY src/AuctionService src/AuctionService
COPY src/Contracts src/Contracts
WORKDIR /app/src/AuctionService
# 在 .NET 里，编译（Build） 是将 C# 代码转换为 .dll 文件，发布（Publish） 是生成可以独立运行的应用（包含依赖）。
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image。生产环境
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# ✅ --from=build → 从第一阶段（build 阶段）拷贝已编译的文件到最终的运行环境
# ✅ 只拷贝 dotnet publish 生成的文件，不包含源代码，提高安全性 & 减少镜像大小
#COPY [选项] <源路径> <目标路径> #.代表目标路径，即最终镜像的 /app 目录
COPY --from=build /app/src/out .
# ENTRYPOINT 指定容器启动时要执行的默认命令。容器启动时执行：dotnet AuctionService.dll
# 这个命令会启动 .NET 应用（相当于 dotnet run）。当你运行 docker run，容器会自动执行这个命令
# 在 dotnet publish 之后，你的应用变成了 .dll 文件，要用 dotnet 运行它。
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]