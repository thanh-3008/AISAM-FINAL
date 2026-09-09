"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";

type Option={id:string;name:string};
interface Row {memberId:string;name:string;contentsCreated:number;creatorPublishedPosts:number;publisherPublishedPosts:number;reviewedSubmissions:number;approvalRate:number|null;completedSchedules:number;pendingSchedules:number;failedSchedules:number;onTimeRate:number|null;failedPublishRate:number|null;turnaroundHours:number|null;postsWithInsights:number;engagement:number|null;impressions:number|null;reach:number|null;engagementRate:number|null;insightsUpdatedAt:string|null}
interface Result {items:Row[];total:number;from:string;to:string;updatedAt:string;unattributedContents:number|null;brands:Option[];teams:Option[];members:Option[];metricDefinitions:Record<string,string>}
const today=()=>new Date().toISOString().slice(0,10);
const thirtyDaysAgo=()=>new Date(Date.now()-30*86400000).toISOString().slice(0,10);
const metric=(value:number|null,suffix="")=>value===null?"Chưa đủ dữ liệu":`${value.toLocaleString("vi-VN")}${suffix}`;

export default function MemberPerformancePage(){
  const [from,setFrom]=useState(thirtyDaysAgo); const [to,setTo]=useState(today);
  const [brandId,setBrand]=useState("");const [teamId,setTeam]=useState("");const [memberId,setMember]=useState("");
  const [page,setPage]=useState(1);const [refresh,setRefresh]=useState(0);
  const [result,setResult]=useState<Result|null>(null); const [error,setError]=useState(""); const [loading,setLoading]=useState(true);
  const [options,setOptions]=useState<{brands:Option[];teams:Option[];members:Option[]}>({brands:[],teams:[],members:[]});
  useEffect(()=>{
    let active=true; setResult(null);setError("");setLoading(true);
    if(!from||!to||from>=to){setError("Chọn ngày bắt đầu trước ngày kết thúc (UTC). Ngày kết thúc không được tính.");setLoading(false);return;}
    const query=new URLSearchParams({from:`${from}T00:00:00Z`,to:`${to}T00:00:00Z`,page:String(page),pageSize:"20"});
    if(brandId)query.set("brandId",brandId);if(teamId)query.set("teamId",teamId);if(memberId)query.set("memberId",memberId);
    apiClient(`/team/member-performance?${query}`).then(response=>{
      if(!active)return; if(!response?.success||!response.data)throw new Error("Không tải được hiệu suất.");
      setResult(response.data);setOptions(response.data);
    }).catch(e=>{if(active)setError(e instanceof Error?e.message:"Không tải được hiệu suất.");})
      .finally(()=>{if(active)setLoading(false);});
    return()=>{active=false;};
  },[from,to,brandId,teamId,memberId,page,refresh]);
  return <main className="p-6 space-y-5">
    <Link href="/team" className="underline">Quay lại Team</Link>
    <h1 className="text-2xl font-bold">Hiệu suất thành viên</h1>
    <p>Số liệu theo quyền hiện tại. Creator chỉ xem bản thân. Ngày theo UTC, không gồm ngày kết thúc.</p>
    <div className="flex flex-wrap gap-4">
      <label>Từ ngày<input aria-label="Từ ngày" type="date" value={from} onChange={e=>{setFrom(e.target.value);setPage(1);}} className="border rounded block p-2" /></label>
      <label>Đến ngày (không gồm)<input aria-label="Đến ngày" type="date" value={to} onChange={e=>{setTo(e.target.value);setPage(1);}} className="border rounded block p-2" /></label>
      {([{label:"Brand",value:brandId,items:options.brands,set:setBrand},{label:"Team",value:teamId,items:options.teams,set:setTeam},{label:"Thành viên",value:memberId,items:options.members,set:setMember}]).map(filter=><label key={filter.label}>{filter.label}<select aria-label={filter.label} value={filter.value} onChange={e=>{filter.set(e.target.value);if(filter.label!=="Thành viên")setMember("");setPage(1);}} className="border rounded block p-2"><option value="">Tất cả được phép</option>{filter.items.map(item=><option key={item.id} value={item.id}>{item.name}</option>)}</select></label>)}
      <button disabled={loading} onClick={()=>setRefresh(n=>n+1)} className="border rounded p-2">Tải lại</button>
    </div>
    {loading&&<p role="status">Đang tải hiệu suất…</p>}
    {error&&<p role="alert" className="text-red-600">{error}</p>}
    {result&&<>
      <p>Cập nhật báo cáo: {new Date(result.updatedAt).toLocaleString("vi-VN")} · {result.total} thành viên trong phạm vi.</p>
      {result.unattributedContents!==null&&<p>Nội dung chưa xác định Creator: {result.unattributedContents}. Không gán vào thành tích cá nhân.</p>}
      {result.items.length===0?<p>Không có thành viên phù hợp trong phạm vi được cấp.</p>:<>
        <div className="overflow-x-auto"><table className="w-full text-sm border-collapse"><caption className="text-left py-2">Nội dung và xuất bản — mỗi đích đăng thành công là một bài</caption><thead><tr>{["Thành viên","Nội dung tạo","Bài từ nội dung của mình","Bài trực tiếp xuất bản","Tỷ lệ duyệt","Thời gian duyệt","Đúng giờ","Lịch lỗi","Chờ / hoàn tất / lỗi"].map(h=><th key={h} className="border p-2 text-left">{h}</th>)}</tr></thead><tbody>{result.items.map(row=><tr key={row.memberId}>
          <th className="border p-2 text-left">{row.name}</th>{[row.contentsCreated,row.creatorPublishedPosts,row.publisherPublishedPosts,metric(row.approvalRate,"%"),metric(row.turnaroundHours," giờ"),metric(row.onTimeRate,"%"),metric(row.failedPublishRate,"%"),`${row.pendingSchedules} / ${row.completedSchedules} / ${row.failedSchedules}`].map((value,index)=><td key={index} className="border p-2">{value}</td>)}</tr>)}</tbody></table></div>
        <div className="overflow-x-auto"><table className="w-full text-sm"><caption className="text-left py-2">Insights lũy kế mới nhất của bài xuất bản trong kỳ — chỉ số theo Creator</caption><thead><tr>{["Thành viên","Bài có insights","Engagement","Impressions","Reach (cộng theo bài)","Engagement rate","Insights cập nhật"].map(h=><th key={h} className="border p-2 text-left">{h}</th>)}</tr></thead><tbody>{result.items.map(row=><tr key={row.memberId}><th className="border p-2 text-left">{row.name}</th>{[row.postsWithInsights,metric(row.engagement),metric(row.impressions),metric(row.reach),metric(row.engagementRate,"%"),row.insightsUpdatedAt?new Date(row.insightsUpdatedAt).toLocaleString("vi-VN"):"Chưa đồng bộ"].map((v,i)=><td key={i} className="border p-2">{v}</td>)}</tr>)}</tbody></table></div>
        <section aria-label="Biểu đồ nội dung tạo" className="space-y-2"><h2 className="font-semibold">Nội dung tạo theo thành viên trong trang</h2>{result.items.map(row=><div key={row.memberId} className="flex gap-3 items-center"><span className="w-40 truncate">{row.name}</span><meter aria-label={`${row.name}: nội dung tạo`} min={0} max={Math.max(1,...result.items.map(r=>r.contentsCreated))} value={row.contentsCreated} className="w-64" /><span>{row.contentsCreated}</span></div>)}</section>
      </>}
      <div className="flex gap-4"><button disabled={page<=1} onClick={()=>setPage(p=>p-1)}>Trang trước</button><span>Trang {page}</span><button disabled={page*20>=result.total} onClick={()=>setPage(p=>p+1)}>Trang sau</button></div>
      <details className="border rounded p-4"><summary>Công thức và giới hạn dữ liệu</summary><p className="py-2">Thiếu mẫu số hoặc timestamp trả “Chưa đủ dữ liệu”. Đúng giờ: ±5 phút trên lịch đơn đã hoàn tất. Lịch lỗi tính theo kết quả cuối, không đếm retry. Lịch lặp và lỗi đăng ngay chưa có lịch sử đủ để tính. Reach không phải số người duy nhất toàn workspace.</p><dl>{Object.entries(result.metricDefinitions).map(([key,value])=><div key={key} className="py-2"><dt className="font-semibold">{key}</dt><dd>{value}</dd></div>)}</dl></details>
    </>}
  </main>;
}
