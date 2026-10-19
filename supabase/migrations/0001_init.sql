-- TimeGate content schema. Source of truth: Supabase = historical CONTENT (read-only to clients);
-- Firebase = auth + per-user progress (see docs/SERVICES.md). Not applied/tested yet.
create table if not exists public.countries (
  id text primary key, iso_code text not null, name_ar text not null, name_en text not null);
create table if not exists public.civilizations (
  id text primary key, name_ar text not null, name_en text not null,
  year_from int, year_to int, period_confidence text check (period_confidence in ('documented','approximate','artistic_reconstruction')));
create table if not exists public.packs (
  id text primary key, country_id text references public.countries(id), civilization_id text references public.civilizations(id),
  pack_version text not null, min_app_version text not null, status text not null check (status in ('planned','playable')),
  size_bytes bigint, sha256 text, published boolean not null default false);
create table if not exists public.sites (
  id text primary key, pack_id text not null references public.packs(id) on delete cascade,
  name_ar text not null, name_en text not null, site_type text not null, summary_ar text, summary_en text,
  confidence text not null check (confidence in ('documented','approximate','artistic_reconstruction')));
create table if not exists public.references_ (
  id bigint generated always as identity primary key, site_id text not null references public.sites(id) on delete cascade,
  title text not null, url text not null, retrieved date, verified boolean not null default false);
create table if not exists public.asset_register (
  id text primary key, name text not null, site_id text references public.sites(id), source_url text, author text,
  license text not null, attribution text, file_format text, size_bytes bigint, verified_on date, status text not null default 'needed');
create index if not exists sites_pack_idx on public.sites(pack_id);
create index if not exists refs_site_idx on public.references_(site_id);

alter table public.countries enable row level security;
alter table public.civilizations enable row level security;
alter table public.packs enable row level security;
alter table public.sites enable row level security;
alter table public.references_ enable row level security;
alter table public.asset_register enable row level security;
-- Clients (anon key) may read published content only; writes require service role (server-side only).
create policy read_countries on public.countries for select using (true);
create policy read_civs on public.civilizations for select using (true);
create policy read_packs on public.packs for select using (published);
create policy read_sites on public.sites for select using (exists (select 1 from public.packs p where p.id = pack_id and p.published));
create policy read_refs on public.references_ for select using (exists (select 1 from public.sites s join public.packs p on p.id = s.pack_id where s.id = site_id and p.published));
-- asset_register: no client policy => not readable by anon (internal).
