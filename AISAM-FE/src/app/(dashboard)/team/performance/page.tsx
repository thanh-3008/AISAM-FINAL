"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { apiClient } from "@/lib/apiClient";
import { inclusiveToExclusiveUtc } from "@/lib/dateRanges";

type Option={id:string;name:string};
interface Row {memberId:string;name:string;contentsCreated:number;creatorPublishedPosts:number;publisherPublishedPosts:number;reviewedSubmissions:number;approvalRate?:number|null;completedSchedules:number;pendingSchedules:number;failedSchedules:number;onTimeRate?:number|null;failedPublishRate?:number|null;turnaroundHours?:number|null;postsWithInsights:number;engagement?:number|null;impressions?:number|null;reach?:number|null;engagementRate?:number|null;insightsUpdatedAt?:string|null}
interface Result {items:Row[];total:number;from:string;to:string;updatedAt:string;unattributedContents?:number|null;brands:Option[];teams:Option[];members:Option[];metricDefinitions:Record<string,string>}
const today=()=>new Date().toISOString().slice(0,10);
const thirtyDaysAgo=()=>new Date(Date.now()-30*86400000).toISOString().slice(0,10);
const metric=(value:number|null|undefined,suffix="")=>typeof value!=="number"||!Number.isFinite(value)?"Insufficient data":`${value.toLocaleString("vi-VN")}${suffix}`;

export default function MemberPerformancePage(){
  const [from,setFrom]=useState(thirtyDaysAgo); const [to,setTo]=useState(today);
  const [brandId,setBrand]=useState("");const [teamId,setTeam]=useState("");const [memberId,setMember]=useState("");
  const [page,setPage]=useState(1);const [refresh,setRefresh]=useState(0);
  const [result,setResult]=useState<Result|null>(null); const [error,setError]=useState(""); const [loading,setLoading]=useState(true);
  const [options,setOptions]=useState<{brands:Option[];teams:Option[];members:Option[]}>({brands:[],teams:[],members:[]});
  useEffect(()=>{
    let active=true; setResult(null);setError("");setLoading(true);
    if(!from||!to||from>to){setError("Start date must be on or before end date (UTC).");setLoading(false);return;}
    const query=new URLSearchParams({from:`${from}T00:00:00Z`,to:inclusiveToExclusiveUtc(to),page:String(page),pageSize:"20"});
    if(brandId)query.set("brandId",brandId);if(teamId)query.set("teamId",teamId);if(memberId)query.set("memberId",memberId);
    apiClient(`/team/member-performance?${query}`).then(response=>{
      if(!active)return; if(!response?.success||!response.data)throw new Error("Unable to load performance data.");
      setResult(response.data);setOptions(response.data);
    }).catch(e=>{if(active)setError(e instanceof Error?e.message:"Unable to load performance data.");})
      .finally(()=>{if(active)setLoading(false);});
    return()=>{active=false;};
  },[from,to,brandId,teamId,memberId,page,refresh]);
  return <main className="mx-auto w-full max-w-[1600px] space-y-6 px-4 py-6 sm:px-8 lg:py-8 text-on-surface">
    <Link href="/team" className="inline-flex items-center gap-2 text-sm font-medium text-on-surface-variant hover:text-primary"><span aria-hidden="true">←</span> Back to Team</Link>
    <header className="flex flex-wrap items-center justify-between gap-4"><div className="flex items-center gap-4"><span aria-hidden="true" className="material-symbols-outlined flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10 text-primary !text-3xl">monitoring</span><div><p className="mb-1 text-xs font-semibold uppercase tracking-widest text-primary">Team insights</p><h1 className="text-2xl font-bold tracking-tight sm:text-3xl">Member Performance</h1></div></div><span className="rounded-full border border-primary/15 bg-primary/5 px-3 py-1.5 text-xs font-medium text-primary">Scoped access</span></header>
    <p className="text-sm leading-6 text-on-surface-variant">Track team contributions, publishing progress, and content performance in one report.</p>
    <div className="grid grid-cols-1 items-end gap-4 rounded-2xl border border-outline-variant/40 bg-surface p-5 shadow-sm sm:grid-cols-2 xl:grid-cols-6 [&_label]:text-xs [&_label]:font-semibold [&_label]:text-on-surface-variant">
      <label>From date<input aria-label="From date" type="date" value={from} onChange={e=>{setFrom(e.target.value);setPage(1);}} className="mt-2 block w-full min-w-0 rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal text-on-surface outline-none focus:border-primary focus:ring-2 focus:ring-primary/15" /></label>
      <label>To date (inclusive)<input aria-label="To date" type="date" value={to} onChange={e=>{setTo(e.target.value);setPage(1);}} className="mt-2 block w-full min-w-0 rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal text-on-surface outline-none focus:border-primary focus:ring-2 focus:ring-primary/15" /></label>
      {([{label:"Brand",value:brandId,items:options.brands,set:setBrand},{label:"Team",value:teamId,items:options.teams,set:setTeam},{label:"Member",value:memberId,items:options.members,set:setMember}]).map(filter=><label key={filter.label}>{filter.label}<select aria-label={filter.label} value={filter.value} onChange={e=>{filter.set(e.target.value);if(filter.label!=="Member")setMember("");setPage(1);}} className="mt-2 block w-full min-w-0 rounded-xl border border-outline-variant/50 bg-surface-container-low px-3 py-3 text-sm font-normal text-on-surface outline-none focus:border-primary focus:ring-2 focus:ring-primary/15"><option value="">All allowed</option>{filter.items.map(item=><option key={item.id} value={item.id}>{item.name}</option>)}</select></label>)}
      <button disabled={loading} onClick={()=>setRefresh(n=>n+1)} className="rounded-xl bg-primary px-5 py-3 text-sm font-semibold text-white shadow-sm transition hover:opacity-90 disabled:opacity-50">Reload</button>
    </div>
    {loading&&<div role="status" className="rounded-2xl border border-outline-variant/40 bg-surface p-12 text-center text-sm text-on-surface-variant"><span className="mx-auto mb-4 block h-7 w-7 animate-spin rounded-full border-2 border-primary/20 border-t-primary" />Loading performance…</div>}
    {error&&<p role="alert" className="rounded-xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700">{error}</p>}
    {result&&<>
      <section aria-label="Performance overview" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {[
          {label:"Members",value:result.total,icon:"group",note:"Within filter scope",color:"bg-blue-50 text-blue-600"},
          {label:"Contents created",value:result.items.reduce((sum,row)=>sum+row.contentsCreated,0),icon:"edit_document",note:"By members on this page",color:"bg-violet-50 text-violet-600"},
          {label:"Published posts",value:result.items.reduce((sum,row)=>sum+row.creatorPublishedPosts,0),icon:"send",note:"By creators on this page",color:"bg-emerald-50 text-emerald-600"},
          {label:"Pending schedules",value:result.items.reduce((sum,row)=>sum+row.pendingSchedules,0),icon:"schedule",note:"For members on this page",color:"bg-amber-50 text-amber-600"},
        ].map(card=><article key={card.label} className="rounded-2xl border border-outline-variant/40 bg-surface p-5 shadow-sm">
          <div className="mb-4 flex items-center justify-between gap-3"><p className="text-sm font-medium text-on-surface-variant">{card.label}</p><span aria-hidden="true" className={`material-symbols-outlined flex h-10 w-10 items-center justify-center rounded-xl ${card.color}`}>{card.icon}</span></div>
          <p className="text-3xl font-bold tracking-tight tabular-nums">{metric(card.value)}</p><p className="mt-2 text-xs text-on-surface-variant">{card.note}</p>
        </article>)}
      </section>
      <p className="text-xs text-on-surface-variant">Date range in UTC, including the entire end date. Creators only view their own data.</p>
      <p className="text-xs text-on-surface-variant">Report updated: {new Date(result.updatedAt).toLocaleString("vi-VN")} · {result.total} members in scope.</p>
      {result.unattributedContents!=null&&<p className="rounded-xl border border-amber-200/60 bg-amber-50 px-4 py-3 text-sm text-amber-800">Contents with unattributed Creator: {result.unattributedContents}. Not assigned to individual metrics.</p>}
      {result.items.length===0?<div className="rounded-2xl border border-dashed border-outline-variant bg-surface px-6 py-16 text-center"><span aria-hidden="true" className="material-symbols-outlined mb-3 !text-4xl text-primary/50">group</span><h2 className="font-semibold">No matching results</h2><p className="mt-2 text-sm text-on-surface-variant">Try changing the date range or member filter.</p></div>:<>
        <div className="overflow-x-auto rounded-2xl border border-outline-variant/40 bg-surface p-2 shadow-sm sm:p-4 [&_tbody_tr]:transition-colors [&_tbody_tr:hover]:bg-primary/5 [&_tbody_tr:last-child_td]:border-0 [&_tbody_tr:last-child_th]:border-0"><table className="w-full min-w-[1050px] text-sm border-collapse"><caption className="px-3 pb-5 pt-2 text-left text-base font-semibold">Content and publishing — each successful target is one post</caption><thead className="bg-surface-container-low text-xs"><tr>{["Member","Contents created","Posts from own content","Directly published posts","Approval rate","Turnaround time","On time","Failed schedules","Pending / completed / failed"].map(h=><th key={h} className="border-b border-outline-variant/40 px-4 py-4 text-left font-medium text-on-surface-variant">{h}</th>)}</tr></thead><tbody>{result.items.map(row=><tr key={row.memberId}>
          <th className="border-b border-outline-variant/40 px-4 py-4 text-left font-medium text-on-surface-variant">{row.name}</th>{[row.contentsCreated,row.creatorPublishedPosts,row.publisherPublishedPosts,metric(row.approvalRate,"%"),metric(row.turnaroundHours," hrs"),metric(row.onTimeRate,"%"),metric(row.failedPublishRate,"%"),`${row.pendingSchedules} / ${row.completedSchedules} / ${row.failedSchedules}`].map((value,index)=><td key={index} className="whitespace-nowrap border-b border-outline-variant/30 px-4 py-4 text-sm tabular-nums">{value}</td>)}</tr>)}</tbody></table></div>
        <div className="overflow-x-auto rounded-2xl border border-outline-variant/40 bg-surface p-2 shadow-sm sm:p-4 [&_tbody_tr]:transition-colors [&_tbody_tr:hover]:bg-primary/5 [&_tbody_tr:last-child_td]:border-0 [&_tbody_tr:last-child_th]:border-0"><table className="w-full min-w-[950px] text-sm"><caption className="px-3 pb-5 pt-2 text-left text-base font-semibold">Latest cumulative insights for posts published in period — metrics by Creator</caption><thead className="bg-surface-container-low text-xs"><tr>{["Member","Posts with insights","Engagement","Impressions","Reach (summed across posts)","Engagement rate","Insights updated"].map(h=><th key={h} className="border-b border-outline-variant/40 px-4 py-4 text-left font-medium text-on-surface-variant">{h}</th>)}</tr></thead><tbody>{result.items.map(row=><tr key={row.memberId}><th className="border-b border-outline-variant/40 px-4 py-4 text-left font-medium text-on-surface-variant">{row.name}</th>{[row.postsWithInsights,metric(row.engagement),metric(row.impressions),metric(row.reach),metric(row.engagementRate,"%"),row.insightsUpdatedAt?new Date(row.insightsUpdatedAt).toLocaleString("vi-VN"):"Not synced"].map((v,i)=><td key={i} className="whitespace-nowrap border-b border-outline-variant/30 px-4 py-4 text-sm tabular-nums">{v}</td>)}</tr>)}</tbody></table></div>
        <section aria-label="Content creation chart" className="space-y-5 rounded-2xl border border-outline-variant/40 bg-surface p-6 shadow-sm"><div><h2 className="text-base font-semibold">Content contribution</h2><p className="mt-1 text-xs text-on-surface-variant">Compare content creation volume across members on current page.</p></div>{result.items.map(row=><div key={row.memberId} className="flex gap-3 items-center"><span className="w-28 shrink-0 truncate text-sm sm:w-40">{row.name}</span><meter aria-label={`${row.name}: content created`} min={0} max={Math.max(1,...result.items.map(r=>r.contentsCreated))} value={row.contentsCreated} className="h-3 w-full max-w-xl accent-blue-600" /><span>{row.contentsCreated}</span></div>)}</section>
      </>}
      <div className="flex items-center justify-end gap-4 text-sm [&_button]:rounded-xl [&_button]:border [&_button]:border-outline-variant/50 [&_button]:bg-surface [&_button]:px-4 [&_button]:py-2.5 [&_button:disabled]:opacity-40"><button disabled={page<=1} onClick={()=>setPage(p=>p-1)}>Previous</button><span>Page {page}</span><button disabled={page*20>=result.total} onClick={()=>setPage(p=>p+1)}>Next</button></div>
      <details className="rounded-2xl border border-outline-variant/40 bg-surface p-5 text-sm text-on-surface-variant [&_summary]:cursor-pointer [&_summary]:font-semibold [&_summary]:text-on-surface"><summary>Formulas and data limitations</summary><p className="py-2">Missing denominator or timestamp returns &quot;Insufficient data&quot;. On time: ±5 mins on completed single schedules. Failed schedules count final outcomes, excluding retries. Recurring schedules and instant publish errors have insufficient history. Reach is not deduplicated across the workspace.</p><dl>{Object.entries(result.metricDefinitions).map(([key,value])=><div key={key} className="py-2"><dt className="font-semibold">{key}</dt><dd>{value}</dd></div>)}</dl></details>
    </>}
  </main>;
}
