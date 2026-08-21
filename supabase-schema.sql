-- Gestion App — schéma de base de données Supabase
-- À coller et exécuter dans : Supabase Dashboard > SQL Editor > New query > Run

create table if not exists public.clients (
  id text primary key,
  name text not null,
  email text not null default '',
  phone text not null default '',
  notes text not null default '',
  created_at bigint not null,
  updated_at bigint not null
);

create table if not exists public.revenus (
  id text primary key,
  date date not null,
  client_id text references public.clients(id) on delete set null,
  montant numeric not null,
  categorie text not null,
  description text not null default '',
  created_at bigint not null,
  updated_at bigint not null
);

create table if not exists public.depenses (
  id text primary key,
  date date not null,
  montant numeric not null,
  categorie text not null,
  description text not null default '',
  created_at bigint not null,
  updated_at bigint not null
);

create table if not exists public.vehicules (
  id text primary key,
  name text not null,
  make text not null default '',
  model text not null default '',
  year text not null default '',
  plate text not null default '',
  notes text not null default '',
  created_at bigint not null,
  updated_at bigint not null
);

-- Optional link from a revenu/depense to a vehicule, used by the
-- Rentabilité tab. Added via ALTER so this stays safe to re-run even
-- if revenus/depenses already existed before vehicules did.
alter table public.revenus add column if not exists vehicule_id text references public.vehicules(id) on delete set null;
alter table public.depenses add column if not exists vehicule_id text references public.vehicules(id) on delete set null;

-- Extra identification fields on vehicules (Info véhicule).
alter table public.vehicules add column if not exists vin text not null default '';
alter table public.vehicules add column if not exists color text not null default '';
alter table public.vehicules add column if not exists mileage numeric;
alter table public.vehicules add column if not exists purchase_date date;

create table if not exists public.maintenances (
  id text primary key,
  vehicule_id text not null references public.vehicules(id) on delete cascade,
  type text not null default 'Autre',
  date date,
  next_due_date date,
  notes text not null default '',
  created_at bigint not null,
  updated_at bigint not null
);

create table if not exists public.inventaire (
  id text primary key,
  name text not null,
  quantity numeric not null default 0,
  unit text not null default '',
  category text not null default '',
  min_threshold numeric,
  notes text not null default '',
  created_at bigint not null,
  updated_at bigint not null
);

-- Optional link from a revenu/depense to an inventaire item ("produit"),
-- also used by the Rentabilité tab.
alter table public.revenus add column if not exists produit_id text references public.inventaire(id) on delete set null;
alter table public.depenses add column if not exists produit_id text references public.inventaire(id) on delete set null;

create table if not exists public.settings (
  id int primary key default 1,
  company_name text not null default 'Mon entreprise',
  currency text not null default 'CAD',
  constraint settings_singleton check (id = 1)
);

insert into public.settings (id, company_name, currency)
values (1, 'Mon entreprise', 'CAD')
on conflict (id) do nothing;

-- Row Level Security : accès public en lecture/écriture via la clé publique
-- (aucun système de compte dans l'appli pour l'instant — voir l'avertissement
-- donné dans la conversation : quiconque a le lien peut lire/modifier les données).
alter table public.clients enable row level security;
alter table public.revenus enable row level security;
alter table public.depenses enable row level security;
alter table public.vehicules enable row level security;
alter table public.inventaire enable row level security;
alter table public.maintenances enable row level security;
alter table public.settings enable row level security;

drop policy if exists "public access" on public.clients;
create policy "public access" on public.clients for all using (true) with check (true);

drop policy if exists "public access" on public.revenus;
create policy "public access" on public.revenus for all using (true) with check (true);

drop policy if exists "public access" on public.depenses;
create policy "public access" on public.depenses for all using (true) with check (true);

drop policy if exists "public access" on public.vehicules;
create policy "public access" on public.vehicules for all using (true) with check (true);

drop policy if exists "public access" on public.inventaire;
create policy "public access" on public.inventaire for all using (true) with check (true);

drop policy if exists "public access" on public.maintenances;
create policy "public access" on public.maintenances for all using (true) with check (true);

drop policy if exists "public access" on public.settings;
create policy "public access" on public.settings for all using (true) with check (true);

grant all on public.clients, public.revenus, public.depenses, public.vehicules, public.inventaire, public.maintenances, public.settings to anon, authenticated;
