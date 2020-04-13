# Qbcode.Aggregate
感谢原作者提供的源码，Bumblebee网关的路由聚合插件，改编自官方插件源码
## 加载插件
```
g = new Gateway();
....省略一万个字...
 g.LoadPlugin(
  typeof(Qbcode.Aggregate.Plugin).Assembly
  );
```
## 进入管理界面配置插件

## 配置格式
- **Actions** 是一些等待被请求的地址
- **Aggregates** 是聚合配置
- 如下 , 
- 当经过网关请求  /api/login/get 时会得到 auth,score,vip三个对应的接口数据
```
{
    "Actions":[
        {
            "Name":"auth",
            "Host":"http://localhost:8080",
            "Url":"/api/user/info"
        },
        {
            "Name":"score",
            "Host":"http://localhost:8081",
            "Url":"/api/score/info"
        },
        {
            "Name":"vip",
            "Host":"http://localhost:8082",
            "Url":"/api/vip/info"
        }
    ],
    "Aggregates":[
        {
            "Url":"/api/login/get",
            "Names":["auth","score","vip"],
            "OutputError":false
        }
    ]
}
```
