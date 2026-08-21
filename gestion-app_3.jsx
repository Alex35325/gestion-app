import React, { useState, useEffect, useMemo, useCallback } from "react";
import {
  LayoutDashboard, Users, TrendingUp, TrendingDown, Settings, Menu, X,
  Plus, Pencil, Trash2, Search, ChevronLeft, ChevronRight, AlertCircle,
  CheckCircle2, ArrowUpRight, ArrowDownRight, Wallet, RotateCcw, Building2
} from "lucide-react";
import {
  ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend
} from "recharts";

const REVENUE_CATEGORIES = ["Vente", "Service", "Abonnement", "Autre"];
const EXPENSE_CATEGORIES = ["Loyer", "Fournitures", "Salaires", "Marketing", "Logiciels", "Autre"];
const DEFAULT_SETTINGS = { companyName: "Mon entreprise", currency: "CAD" };

const NAV_ITEMS = [
  { id: "dashboard", label: "Tableau de bord", icon: LayoutDashboard },
  { id: "clients", label: "Clients", icon: Users },
  { id: "revenus", label: "Revenus", icon: TrendingUp },
  { id: "depenses", label: "Dépenses", icon: TrendingDown },
  { id: "parametres", label: "Paramètres", icon: Settings },
];

function generateId() {
  return `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`;
}

function todayStr() {
  return new Date().toISOString().slice(0, 10);
}

function formatCurrency(amount, currency) {
  try {
    return new Intl.NumberFormat("fr-CA", { style: "currency", currency: currency || "CAD" }).format(amount || 0);
  } catch (e) {
    return `${(amount || 0).toFixed(2)} $`;
  }
}

function formatDate(dateStr) {
  try {
    return new Date(dateStr).toLocaleDateString("fr-FR", { day: "2-digit", month: "short", year: "numeric" });
  } catch (e) {
    return dateStr;
  }
}

function buildDemoData() {
  const now = new Date();
  const monthsAgo = (n, day) => {
    const d = new Date(now.getFullYear(), now.getMonth() - n, day || 1);
    return d.toISOString().slice(0, 10);
  };
  const clients = [
    { id: generateId(), name: "Atelier Nordik", email: "contact@ateliernordik.ca", phone: "514-555-0142", notes: "Client régulier, facturation mensuelle.", createdAt: Date.now(), updatedAt: Date.now() },
    { id: generateId(), name: "Boutique Lumen", email: "info@lumen.ca", phone: "438-555-0198", notes: "", createdAt: Date.now(), updatedAt: Date.now() },
    { id: generateId(), name: "Studio Verre & Bois", email: "studio@verrebois.ca", phone: "", notes: "Paiement sur 30 jours.", createdAt: Date.now(), updatedAt: Date.now() },
  ];
  const revenus = [
    { id: generateId(), date: monthsAgo(0, 3), clientId: clients[0].id, montant: 2400, categorie: "Service", description: "Mandat de consultation" },
    { id: generateId(), date: monthsAgo(0, 12), clientId: clients[1].id, montant: 890, categorie: "Vente", description: "Commande #1042" },
    { id: generateId(), date: monthsAgo(1, 5), clientId: clients[2].id, montant: 1650, categorie: "Service", description: "Projet mars" },
    { id: generateId(), date: monthsAgo(1, 20), clientId: clients[0].id, montant: 2400, categorie: "Service", description: "Mandat de consultation" },
    { id: generateId(), date: monthsAgo(2, 8), clientId: clients[1].id, montant: 640, categorie: "Vente", description: "Commande #1021" },
    { id: generateId(), date: monthsAgo(2, 18), clientId: null, montant: 300, categorie: "Autre", description: "Vente comptoir" },
    { id: generateId(), date: monthsAgo(3, 10), clientId: clients[2].id, montant: 1450, categorie: "Service", description: "Projet janvier" },
    { id: generateId(), date: monthsAgo(4, 14), clientId: clients[0].id, montant: 2200, categorie: "Service", description: "Mandat de consultation" },
  ];
  const depenses = [
    { id: generateId(), date: monthsAgo(0, 1), categorie: "Loyer", montant: 950, description: "Loyer local" },
    { id: generateId(), date: monthsAgo(0, 6), categorie: "Logiciels", montant: 89, description: "Abonnements SaaS" },
    { id: generateId(), date: monthsAgo(0, 15), categorie: "Marketing", montant: 220, description: "Publicité en ligne" },
    { id: generateId(), date: monthsAgo(1, 1), categorie: "Loyer", montant: 950, description: "Loyer local" },
    { id: generateId(), date: monthsAgo(1, 11), categorie: "Fournitures", montant: 310, description: "Matériel" },
    { id: generateId(), date: monthsAgo(2, 1), categorie: "Loyer", montant: 950, description: "Loyer local" },
    { id: generateId(), date: monthsAgo(2, 22), categorie: "Salaires", montant: 1800, description: "Sous-traitance" },
    { id: generateId(), date: monthsAgo(3, 1), categorie: "Loyer", montant: 950, description: "Loyer local" },
  ];
  return { clients, revenus, depenses };
}

function inPeriod(dateStr, period, now) {
  const d = new Date(dateStr);
  if (period === "mois") return d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth();
  if (period === "mois-dernier") {
    const pm = new Date(now.getFullYear(), now.getMonth() - 1, 1);
    return d.getFullYear() === pm.getFullYear() && d.getMonth() === pm.getMonth();
  }
  if (period === "annee") return d.getFullYear() === now.getFullYear();
  return true;
}

function sumMontant(list) {
  return list.reduce((s, item) => s + (Number(item.montant) || 0), 0);
}

/* ---------- Small shared UI pieces ---------- */

function Toast({ toast }) {
  if (!toast) return null;
  const isError = toast.type === "error";
  return (
    <div
      role="status"
      className={`fixed bottom-4 right-4 z-[100] flex items-center gap-2 rounded-lg px-4 py-3 shadow-lg text-sm font-medium
        ${isError ? "bg-red-600 text-white" : "bg-slate-900 text-white"}`}
    >
      {isError ? <AlertCircle size={16} /> : <CheckCircle2 size={16} />}
      {toast.message}
    </div>
  );
}

function ConfirmDialog({ state, onCancel }) {
  if (!state) return null;
  return (
    <div className="fixed inset-0 z-[90] flex items-center justify-center bg-slate-900/40 px-4" onClick={onCancel}>
      <div className="w-full max-w-sm rounded-xl bg-white p-5 shadow-xl" onClick={(e) => e.stopPropagation()}>
        <div className="flex items-start gap-3">
          <div className="mt-0.5 shrink-0 rounded-full bg-red-50 p-2 text-red-600"><AlertCircle size={18} /></div>
          <div>
            <p className="font-semibold text-slate-900">Confirmer la suppression</p>
            <p className="mt-1 text-sm text-slate-500">{state.message}</p>
          </div>
        </div>
        <div className="mt-5 flex justify-end gap-2">
          <button onClick={onCancel} className="rounded-md px-3 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100">
            Annuler
          </button>
          <button
            onClick={state.onConfirm}
            className="rounded-md bg-red-600 px-3 py-2 text-sm font-medium text-white hover:bg-red-700"
          >
            Supprimer
          </button>
        </div>
      </div>
    </div>
  );
}

function Modal({ title, onClose, children }) {
  return (
    <div className="fixed inset-0 z-[80] flex items-center justify-center bg-slate-900/40 px-4 py-6" onClick={onClose}>
      <div
        className="max-h-full w-full max-w-lg overflow-y-auto rounded-xl bg-white shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
          <h3 className="font-semibold text-slate-900">{title}</h3>
          <button onClick={onClose} className="rounded-md p-1 text-slate-400 hover:bg-slate-100 hover:text-slate-600">
            <X size={18} />
          </button>
        </div>
        <div className="px-5 py-4">{children}</div>
      </div>
    </div>
  );
}

function Field({ label, error, children, required }) {
  return (
    <label className="mb-3 block">
      <span className="mb-1 block text-sm font-medium text-slate-700">
        {label}{required && <span className="text-red-500"> *</span>}
      </span>
      {children}
      {error && <span className="mt-1 block text-xs text-red-600">{error}</span>}
    </label>
  );
}

function EmptyState({ icon: Icon, title, subtitle, actionLabel, onAction }) {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-slate-200 py-14 text-center">
      <div className="mb-3 rounded-full bg-slate-100 p-3 text-slate-400"><Icon size={22} /></div>
      <p className="font-medium text-slate-700">{title}</p>
      <p className="mt-1 text-sm text-slate-400">{subtitle}</p>
      {actionLabel && (
        <button onClick={onAction} className="mt-4 inline-flex items-center gap-1.5 rounded-md bg-emerald-600 px-3.5 py-2 text-sm font-medium text-white hover:bg-emerald-700">
          <Plus size={16} /> {actionLabel}
        </button>
      )}
    </div>
  );
}

function KpiCard({ label, value, changePct, tone }) {
  const positive = changePct !== null && changePct >= 0;
  const toneClasses = tone === "danger" ? "text-red-600" : tone === "neutral" ? "text-slate-900" : "text-slate-900";
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4">
      <p className="text-sm text-slate-500">{label}</p>
      <p className={`mt-1.5 text-2xl font-semibold ${toneClasses}`}>{value}</p>
      {changePct !== null && !isNaN(changePct) && isFinite(changePct) && (
        <p className={`mt-1.5 inline-flex items-center gap-1 text-xs font-medium ${positive ? "text-emerald-700" : "text-red-600"}`}>
          {positive ? <ArrowUpRight size={14} /> : <ArrowDownRight size={14} />}
          {Math.abs(changePct).toFixed(0)}% vs mois dernier
        </p>
      )}
    </div>
  );
}

/* ---------- App ---------- */

export default function App() {
  const [loading, setLoading] = useState(true);
  const [clients, setClients] = useState([]);
  const [revenus, setRevenus] = useState([]);
  const [depenses, setDepenses] = useState([]);
  const [settings, setSettings] = useState(DEFAULT_SETTINGS);

  const [activeView, setActiveView] = useState("dashboard");
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const [dashboardPeriod, setDashboardPeriod] = useState("mois");

  const [toast, setToast] = useState(null);
  const [confirmState, setConfirmState] = useState(null);
  const [clientModal, setClientModal] = useState(null);
  const [revenuModal, setRevenuModal] = useState(null);
  const [depenseModal, setDepenseModal] = useState(null);
  const [search, setSearch] = useState("");

  const showToast = useCallback((type, message) => {
    setToast({ type, message });
    setTimeout(() => setToast(null), 3000);
  }, []);

  useEffect(() => {
    (async () => {
      try {
        const results = await Promise.allSettled([
          window.storage.get("clients"),
          window.storage.get("revenus"),
          window.storage.get("depenses"),
          window.storage.get("settings"),
        ]);
        const parse = (r) => (r.status === "fulfilled" && r.value ? JSON.parse(r.value.value) : null);
        let clientsData = parse(results[0]);
        let revenusData = parse(results[1]);
        let depensesData = parse(results[2]);
        let settingsData = parse(results[3]);

        if (!clientsData && !revenusData && !depensesData) {
          const demo = buildDemoData();
          clientsData = demo.clients;
          revenusData = demo.revenus;
          depensesData = demo.depenses;
          await Promise.allSettled([
            window.storage.set("clients", JSON.stringify(clientsData)),
            window.storage.set("revenus", JSON.stringify(revenusData)),
            window.storage.set("depenses", JSON.stringify(depensesData)),
          ]);
        }
        setClients(clientsData || []);
        setRevenus(revenusData || []);
        setDepenses(depensesData || []);
        setSettings(settingsData || DEFAULT_SETTINGS);
      } catch (e) {
        showToast("error", "Impossible de charger les données enregistrées.");
      } finally {
        setLoading(false);
      }
    })();
  }, [showToast]);

  const persist = useCallback(async (key, data) => {
    try {
      const res = await window.storage.set(key, JSON.stringify(data));
      if (!res) throw new Error("save failed");
      return true;
    } catch (e) {
      showToast("error", "L'enregistrement a échoué. Réessayez.");
      return false;
    }
  }, [showToast]);

  /* ---- Clients CRUD ---- */
  const saveClient = async (data, id) => {
    const updated = id
      ? clients.map((c) => (c.id === id ? { ...c, ...data, updatedAt: Date.now() } : c))
      : [...clients, { id: generateId(), ...data, createdAt: Date.now(), updatedAt: Date.now() }];
    if (await persist("clients", updated)) {
      setClients(updated);
      setClientModal(null);
      showToast("success", id ? "Client mis à jour." : "Client ajouté.");
    }
  };
  const deleteClient = (client) => {
    setConfirmState({
      message: `Supprimer « ${client.name} » ? Cette action est irréversible.`,
      onConfirm: async () => {
        const updated = clients.filter((c) => c.id !== client.id);
        if (await persist("clients", updated)) {
          setClients(updated);
          showToast("success", "Client supprimé.");
        }
        setConfirmState(null);
      },
    });
  };

  /* ---- Revenus CRUD ---- */
  const saveRevenu = async (data, id) => {
    const updated = id
      ? revenus.map((r) => (r.id === id ? { ...r, ...data, updatedAt: Date.now() } : r))
      : [...revenus, { id: generateId(), ...data, createdAt: Date.now(), updatedAt: Date.now() }];
    if (await persist("revenus", updated)) {
      setRevenus(updated);
      setRevenuModal(null);
      showToast("success", id ? "Revenu mis à jour." : "Revenu ajouté.");
    }
  };
  const deleteRevenu = (item) => {
    setConfirmState({
      message: `Supprimer cette entrée de ${formatCurrency(item.montant, settings.currency)} ? Cette action est irréversible.`,
      onConfirm: async () => {
        const updated = revenus.filter((r) => r.id !== item.id);
        if (await persist("revenus", updated)) {
          setRevenus(updated);
          showToast("success", "Revenu supprimé.");
        }
        setConfirmState(null);
      },
    });
  };

  /* ---- Dépenses CRUD ---- */
  const saveDepense = async (data, id) => {
    const updated = id
      ? depenses.map((d) => (d.id === id ? { ...d, ...data, updatedAt: Date.now() } : d))
      : [...depenses, { id: generateId(), ...data, createdAt: Date.now(), updatedAt: Date.now() }];
    if (await persist("depenses", updated)) {
      setDepenses(updated);
      setDepenseModal(null);
      showToast("success", id ? "Dépense mise à jour." : "Dépense ajoutée.");
    }
  };
  const deleteDepense = (item) => {
    setConfirmState({
      message: `Supprimer cette dépense de ${formatCurrency(item.montant, settings.currency)} ? Cette action est irréversible.`,
      onConfirm: async () => {
        const updated = depenses.filter((d) => d.id !== item.id);
        if (await persist("depenses", updated)) {
          setDepenses(updated);
          showToast("success", "Dépense supprimée.");
        }
        setConfirmState(null);
      },
    });
  };

  /* ---- Settings ---- */
  const saveSettings = async (data) => {
    if (await persist("settings", data)) {
      setSettings(data);
      showToast("success", "Paramètres enregistrés.");
    }
  };
  const resetDemoData = () => {
    setConfirmState({
      message: "Ceci remplace toutes les données actuelles par de nouvelles données de démonstration.",
      onConfirm: async () => {
        const demo = buildDemoData();
        await Promise.allSettled([
          persist("clients", demo.clients),
          persist("revenus", demo.revenus),
          persist("depenses", demo.depenses),
        ]);
        setClients(demo.clients);
        setRevenus(demo.revenus);
        setDepenses(demo.depenses);
        setConfirmState(null);
        showToast("success", "Données de démonstration réinitialisées.");
      },
    });
  };
  const clearAllData = () => {
    setConfirmState({
      message: "Ceci supprime définitivement tous les clients, revenus et dépenses. Utilisez cette option quand vous êtes prêt à utiliser l'application pour de vrai.",
      onConfirm: async () => {
        await Promise.allSettled([persist("clients", []), persist("revenus", []), persist("depenses", [])]);
        setClients([]);
        setRevenus([]);
        setDepenses([]);
        setConfirmState(null);
        showToast("success", "Toutes les données ont été effacées.");
      },
    });
  };

  /* ---- Derived data ---- */
  const now = useMemo(() => new Date(), []);
  const filteredRevenus = useMemo(() => revenus.filter((r) => inPeriod(r.date, dashboardPeriod, now)), [revenus, dashboardPeriod, now]);
  const filteredDepenses = useMemo(() => depenses.filter((d) => inPeriod(d.date, dashboardPeriod, now)), [depenses, dashboardPeriod, now]);
  const totalRevenus = sumMontant(filteredRevenus);
  const totalDepenses = sumMontant(filteredDepenses);
  const benefice = totalRevenus - totalDepenses;
  const marge = totalRevenus > 0 ? (benefice / totalRevenus) * 100 : 0;

  const curMonthRev = sumMontant(revenus.filter((r) => inPeriod(r.date, "mois", now)));
  const prevMonthRev = sumMontant(revenus.filter((r) => inPeriod(r.date, "mois-dernier", now)));
  const curMonthDep = sumMontant(depenses.filter((d) => inPeriod(d.date, "mois", now)));
  const prevMonthDep = sumMontant(depenses.filter((d) => inPeriod(d.date, "mois-dernier", now)));
  const pctChange = (cur, prev) => (prev > 0 ? ((cur - prev) / prev) * 100 : cur > 0 ? 100 : null);

  const chartData = useMemo(() => {
    const months = [];
    for (let i = 5; i >= 0; i--) {
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      months.push({ label: d.toLocaleDateString("fr-FR", { month: "short" }), year: d.getFullYear(), month: d.getMonth() });
    }
    return months.map((m) => ({
      name: m.label,
      Revenus: sumMontant(revenus.filter((r) => { const rd = new Date(r.date); return rd.getFullYear() === m.year && rd.getMonth() === m.month; })),
      Dépenses: sumMontant(depenses.filter((d) => { const dd = new Date(d.date); return dd.getFullYear() === m.year && dd.getMonth() === m.month; })),
    }));
  }, [revenus, depenses, now]);

  const recentActivity = useMemo(() => {
    const items = [
      ...revenus.map((r) => ({ ...r, kind: "revenu" })),
      ...depenses.map((d) => ({ ...d, kind: "depense" })),
    ];
    return items.sort((a, b) => new Date(b.date) - new Date(a.date)).slice(0, 6);
  }, [revenus, depenses]);

  const clientName = (id) => clients.find((c) => c.id === id)?.name || "—";

  const filteredClients = clients.filter((c) => c.name.toLowerCase().includes(search.toLowerCase()));

  const navigate = (view) => {
    setActiveView(view);
    setMobileNavOpen(false);
    setSearch("");
  };

  if (loading) {
    return (
      <div className="flex h-full min-h-[500px] items-center justify-center bg-slate-50 font-sans">
        <div className="flex items-center gap-2 text-slate-400">
          <div className="h-4 w-4 animate-spin rounded-full border-2 border-slate-300 border-t-emerald-600" />
          Chargement…
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-full min-h-[640px] bg-slate-50 font-sans text-slate-900">
      {/* Desktop sidebar */}
      <aside className={`hidden shrink-0 flex-col border-r border-slate-200 bg-white transition-all duration-200 md:flex ${sidebarCollapsed ? "w-16" : "w-60"}`}>
        <div className={`flex items-center gap-2 border-b border-slate-100 px-4 py-4 ${sidebarCollapsed ? "justify-center px-2" : ""}`}>
          <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md bg-emerald-600 text-white"><Building2 size={16} /></div>
          {!sidebarCollapsed && <span className="truncate text-sm font-semibold">{settings.companyName}</span>}
        </div>
        <nav className="flex-1 space-y-1 px-2 py-3">
          {NAV_ITEMS.map((item) => {
            const Icon = item.icon;
            const active = activeView === item.id;
            return (
              <button
                key={item.id}
                onClick={() => navigate(item.id)}
                title={sidebarCollapsed ? item.label : undefined}
                className={`flex w-full items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors
                  ${active ? "bg-emerald-50 text-emerald-700" : "text-slate-600 hover:bg-slate-100"}
                  ${sidebarCollapsed ? "justify-center px-0" : ""}`}
              >
                <Icon size={18} />
                {!sidebarCollapsed && item.label}
              </button>
            );
          })}
        </nav>
        <button
          onClick={() => setSidebarCollapsed((v) => !v)}
          className="flex items-center justify-center gap-2 border-t border-slate-100 px-3 py-3 text-xs font-medium text-slate-400 hover:bg-slate-50 hover:text-slate-600"
        >
          {sidebarCollapsed ? <ChevronRight size={16} /> : (<><ChevronLeft size={16} /> Réduire</>)}
        </button>
      </aside>

      {/* Mobile overlay nav */}
      {mobileNavOpen && (
        <div className="fixed inset-0 z-[70] flex md:hidden">
          <div className="absolute inset-0 bg-slate-900/40" onClick={() => setMobileNavOpen(false)} />
          <div className="relative z-10 flex h-full w-64 flex-col bg-white shadow-xl">
            <div className="flex items-center justify-between border-b border-slate-100 px-4 py-4">
              <span className="text-sm font-semibold">{settings.companyName}</span>
              <button onClick={() => setMobileNavOpen(false)} className="text-slate-400"><X size={20} /></button>
            </div>
            <nav className="flex-1 space-y-1 px-2 py-3">
              {NAV_ITEMS.map((item) => {
                const Icon = item.icon;
                const active = activeView === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => navigate(item.id)}
                    className={`flex w-full items-center gap-3 rounded-md px-3 py-2.5 text-sm font-medium
                      ${active ? "bg-emerald-50 text-emerald-700" : "text-slate-600 hover:bg-slate-100"}`}
                  >
                    <Icon size={18} /> {item.label}
                  </button>
                );
              })}
            </nav>
          </div>
        </div>
      )}

      {/* Main column */}
      <div className="flex min-w-0 flex-1 flex-col">
        {/* Mobile top bar */}
        <div className="flex items-center justify-between border-b border-slate-200 bg-white px-4 py-3 md:hidden">
          <button onClick={() => setMobileNavOpen(true)} className="text-slate-500"><Menu size={22} /></button>
          <span className="text-sm font-semibold">{NAV_ITEMS.find((n) => n.id === activeView)?.label}</span>
          <div className="w-[22px]" />
        </div>

        <main className="flex-1 overflow-y-auto px-4 py-5 sm:px-6 lg:px-8 lg:py-7">
          {activeView === "dashboard" && (
            <DashboardView
              settings={settings}
              dashboardPeriod={dashboardPeriod}
              setDashboardPeriod={setDashboardPeriod}
              totalRevenus={totalRevenus}
              totalDepenses={totalDepenses}
              benefice={benefice}
              marge={marge}
              pctChange={pctChange}
              curMonthRev={curMonthRev} prevMonthRev={prevMonthRev}
              curMonthDep={curMonthDep} prevMonthDep={prevMonthDep}
              chartData={chartData}
              recentActivity={recentActivity}
              clientName={clientName}
            />
          )}

          {activeView === "clients" && (
            <ClientsView
              clients={filteredClients}
              search={search}
              setSearch={setSearch}
              onAdd={() => setClientModal({ mode: "add" })}
              onEdit={(c) => setClientModal({ mode: "edit", data: c })}
              onDelete={deleteClient}
            />
          )}

          {activeView === "revenus" && (
            <RevenusView
              revenus={revenus}
              clients={clients}
              settings={settings}
              onAdd={() => setRevenuModal({ mode: "add" })}
              onEdit={(r) => setRevenuModal({ mode: "edit", data: r })}
              onDelete={deleteRevenu}
              clientName={clientName}
            />
          )}

          {activeView === "depenses" && (
            <DepensesView
              depenses={depenses}
              settings={settings}
              onAdd={() => setDepenseModal({ mode: "add" })}
              onEdit={(d) => setDepenseModal({ mode: "edit", data: d })}
              onDelete={deleteDepense}
            />
          )}

          {activeView === "parametres" && (
            <ParametresView
              settings={settings}
              onSave={saveSettings}
              onResetDemo={resetDemoData}
              onClearAll={clearAllData}
            />
          )}
        </main>
      </div>

      {clientModal && (
        <ClientForm
          initial={clientModal.data}
          onCancel={() => setClientModal(null)}
          onSave={(data) => saveClient(data, clientModal.data?.id)}
        />
      )}
      {revenuModal && (
        <RevenuForm
          initial={revenuModal.data}
          clients={clients}
          onCancel={() => setRevenuModal(null)}
          onSave={(data) => saveRevenu(data, revenuModal.data?.id)}
        />
      )}
      {depenseModal && (
        <DepenseForm
          initial={depenseModal.data}
          onCancel={() => setDepenseModal(null)}
          onSave={(data) => saveDepense(data, depenseModal.data?.id)}
        />
      )}

      <ConfirmDialog state={confirmState} onCancel={() => setConfirmState(null)} />
      <Toast toast={toast} />
    </div>
  );
}

/* ---------- Dashboard ---------- */

function DashboardView(props) {
  const {
    settings, dashboardPeriod, setDashboardPeriod, totalRevenus, totalDepenses, benefice, marge,
    pctChange, curMonthRev, prevMonthRev, curMonthDep, prevMonthDep, chartData, recentActivity, clientName,
  } = props;

  const periods = [
    { id: "mois", label: "Ce mois" },
    { id: "mois-dernier", label: "Mois dernier" },
    { id: "annee", label: "Cette année" },
    { id: "tout", label: "Tout" },
  ];

  return (
    <div>
      <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-lg font-semibold text-slate-900">Tableau de bord</h1>
          <p className="text-sm text-slate-500">Vue d'ensemble de {settings.companyName}</p>
        </div>
        <div className="flex rounded-md border border-slate-200 bg-white p-0.5 text-sm">
          {periods.map((p) => (
            <button
              key={p.id}
              onClick={() => setDashboardPeriod(p.id)}
              className={`rounded px-3 py-1.5 font-medium transition-colors ${dashboardPeriod === p.id ? "bg-emerald-600 text-white" : "text-slate-500 hover:text-slate-800"}`}
            >
              {p.label}
            </button>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard label="Revenus" value={formatCurrency(totalRevenus, settings.currency)} changePct={pctChange(curMonthRev, prevMonthRev)} />
        <KpiCard label="Dépenses" value={formatCurrency(totalDepenses, settings.currency)} changePct={pctChange(curMonthDep, prevMonthDep) === null ? null : -pctChange(curMonthDep, prevMonthDep)} />
        <KpiCard label="Bénéfice net" value={formatCurrency(benefice, settings.currency)} changePct={null} tone={benefice < 0 ? "danger" : "neutral"} />
        <KpiCard label="Marge" value={`${marge.toFixed(1)}%`} changePct={null} />
      </div>

      <div className="mt-5 grid grid-cols-1 gap-4 lg:grid-cols-3">
        <div className="rounded-xl border border-slate-200 bg-white p-4 lg:col-span-2">
          <p className="mb-3 text-sm font-semibold text-slate-700">Revenus vs dépenses — 6 derniers mois</p>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData} barGap={4}>
                <CartesianGrid strokeDasharray="3 3" stroke="#E2E8F0" vertical={false} />
                <XAxis dataKey="name" tick={{ fontSize: 12, fill: "#64748B" }} axisLine={{ stroke: "#E2E8F0" }} tickLine={false} />
                <YAxis tick={{ fontSize: 12, fill: "#64748B" }} axisLine={false} tickLine={false} width={40} />
                <Tooltip formatter={(v) => formatCurrency(v, settings.currency)} contentStyle={{ borderRadius: 8, borderColor: "#E2E8F0", fontSize: 13 }} />
                <Legend wrapperStyle={{ fontSize: 12 }} />
                <Bar dataKey="Revenus" fill="#059669" radius={[3, 3, 0, 0]} />
                <Bar dataKey="Dépenses" fill="#DC2626" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4">
          <p className="mb-3 text-sm font-semibold text-slate-700">Activité récente</p>
          {recentActivity.length === 0 ? (
            <p className="py-8 text-center text-sm text-slate-400">Aucune activité pour l'instant.</p>
          ) : (
            <ul className="space-y-3">
              {recentActivity.map((item) => (
                <li key={item.id} className="flex items-center justify-between text-sm">
                  <div className="min-w-0">
                    <p className="truncate font-medium text-slate-700">
                      {item.kind === "revenu" ? clientName(item.clientId) || "Revenu" : item.categorie}
                    </p>
                    <p className="text-xs text-slate-400">{formatDate(item.date)}</p>
                  </div>
                  <span className={`shrink-0 font-semibold ${item.kind === "revenu" ? "text-emerald-700" : "text-red-600"}`}>
                    {item.kind === "revenu" ? "+" : "-"}{formatCurrency(item.montant, settings.currency)}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}

/* ---------- Clients ---------- */

function ClientsView({ clients, search, setSearch, onAdd, onEdit, onDelete }) {
  return (
    <div>
      <ViewHeader title="Clients" subtitle="Vos contacts et clients" actionLabel="Ajouter un client" onAction={onAdd} />
      <SearchBar value={search} onChange={setSearch} placeholder="Rechercher un client…" />
      {clients.length === 0 ? (
        <EmptyState icon={Users} title="Aucun client" subtitle="Ajoutez votre premier client pour commencer." actionLabel="Ajouter un client" onAction={onAdd} />
      ) : (
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-4 py-3 font-medium">Nom</th>
                <th className="hidden px-4 py-3 font-medium sm:table-cell">Courriel</th>
                <th className="hidden px-4 py-3 font-medium md:table-cell">Téléphone</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {clients.map((c) => (
                <tr key={c.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                  <td className="px-4 py-3 font-medium text-slate-800">{c.name}</td>
                  <td className="hidden px-4 py-3 text-slate-500 sm:table-cell">{c.email || "—"}</td>
                  <td className="hidden px-4 py-3 text-slate-500 md:table-cell">{c.phone || "—"}</td>
                  <td className="px-4 py-3 text-right">
                    <RowActions onEdit={() => onEdit(c)} onDelete={() => onDelete(c)} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function ClientForm({ initial, onCancel, onSave }) {
  const [name, setName] = useState(initial?.name || "");
  const [email, setEmail] = useState(initial?.email || "");
  const [phone, setPhone] = useState(initial?.phone || "");
  const [notes, setNotes] = useState(initial?.notes || "");
  const [error, setError] = useState("");

  const submit = (e) => {
    e.preventDefault();
    if (!name.trim()) { setError("Le nom est requis."); return; }
    onSave({ name: name.trim(), email: email.trim(), phone: phone.trim(), notes: notes.trim() });
  };

  return (
    <Modal title={initial ? "Modifier le client" : "Ajouter un client"} onClose={onCancel}>
      <form onSubmit={submit}>
        <Field label="Nom" required error={error}>
          <input className={inputClass(!!error)} value={name} onChange={(e) => { setName(e.target.value); setError(""); }} placeholder="Ex. Atelier Nordik" autoFocus />
        </Field>
        <Field label="Courriel">
          <input type="email" className={inputClass()} value={email} onChange={(e) => setEmail(e.target.value)} placeholder="contact@exemple.ca" />
        </Field>
        <Field label="Téléphone">
          <input className={inputClass()} value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="514-555-0100" />
        </Field>
        <Field label="Notes">
          <textarea className={inputClass()} rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Notes internes…" />
        </Field>
        <FormActions onCancel={onCancel} />
      </form>
    </Modal>
  );
}

/* ---------- Revenus ---------- */

function RevenusView({ revenus, clients, settings, onAdd, onEdit, onDelete, clientName }) {
  const total = sumMontant(revenus);
  return (
    <div>
      <ViewHeader title="Revenus" subtitle={`Total : ${formatCurrency(total, settings.currency)}`} actionLabel="Ajouter un revenu" onAction={onAdd} />
      {revenus.length === 0 ? (
        <EmptyState icon={TrendingUp} title="Aucun revenu enregistré" subtitle="Ajoutez votre première entrée de revenu." actionLabel="Ajouter un revenu" onAction={onAdd} />
      ) : (
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-4 py-3 font-medium">Date</th>
                <th className="hidden px-4 py-3 font-medium sm:table-cell">Client</th>
                <th className="hidden px-4 py-3 font-medium md:table-cell">Catégorie</th>
                <th className="px-4 py-3 font-medium">Montant</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {[...revenus].sort((a, b) => new Date(b.date) - new Date(a.date)).map((r) => (
                <tr key={r.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                  <td className="px-4 py-3 text-slate-600">{formatDate(r.date)}</td>
                  <td className="hidden px-4 py-3 text-slate-600 sm:table-cell">{clientName(r.clientId)}</td>
                  <td className="hidden px-4 py-3 md:table-cell">
                    <span className="rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700">{r.categorie}</span>
                  </td>
                  <td className="px-4 py-3 font-semibold text-emerald-700">{formatCurrency(r.montant, settings.currency)}</td>
                  <td className="px-4 py-3 text-right">
                    <RowActions onEdit={() => onEdit(r)} onDelete={() => onDelete(r)} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function RevenuForm({ initial, clients, onCancel, onSave }) {
  const [date, setDate] = useState(initial?.date || todayStr());
  const [clientId, setClientId] = useState(initial?.clientId || "");
  const [montant, setMontant] = useState(initial?.montant ?? "");
  const [categorie, setCategorie] = useState(initial?.categorie || REVENUE_CATEGORIES[0]);
  const [description, setDescription] = useState(initial?.description || "");
  const [error, setError] = useState("");

  const submit = (e) => {
    e.preventDefault();
    const amount = Number(montant);
    if (!date || !amount || amount <= 0) { setError("La date et un montant valide (supérieur à 0) sont requis."); return; }
    onSave({ date, clientId: clientId || null, montant: amount, categorie, description: description.trim() });
  };

  return (
    <Modal title={initial ? "Modifier le revenu" : "Ajouter un revenu"} onClose={onCancel}>
      <form onSubmit={submit}>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Date" required>
            <input type="date" className={inputClass()} value={date} onChange={(e) => setDate(e.target.value)} />
          </Field>
          <Field label="Montant" required>
            <input type="number" step="0.01" min="0" className={inputClass()} value={montant} onChange={(e) => setMontant(e.target.value)} placeholder="0.00" />
          </Field>
        </div>
        <Field label="Client">
          <select className={inputClass()} value={clientId} onChange={(e) => setClientId(e.target.value)}>
            <option value="">Aucun client</option>
            {clients.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </Field>
        <Field label="Catégorie">
          <select className={inputClass()} value={categorie} onChange={(e) => setCategorie(e.target.value)}>
            {REVENUE_CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
        </Field>
        <Field label="Description">
          <input className={inputClass()} value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Ex. Facture #204" />
        </Field>
        {error && <p className="mb-3 text-xs text-red-600">{error}</p>}
        <FormActions onCancel={onCancel} />
      </form>
    </Modal>
  );
}

/* ---------- Dépenses ---------- */

function DepensesView({ depenses, settings, onAdd, onEdit, onDelete }) {
  const total = sumMontant(depenses);
  return (
    <div>
      <ViewHeader title="Dépenses" subtitle={`Total : ${formatCurrency(total, settings.currency)}`} actionLabel="Ajouter une dépense" onAction={onAdd} />
      {depenses.length === 0 ? (
        <EmptyState icon={TrendingDown} title="Aucune dépense enregistrée" subtitle="Ajoutez votre première dépense." actionLabel="Ajouter une dépense" onAction={onAdd} />
      ) : (
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
                <th className="px-4 py-3 font-medium">Date</th>
                <th className="hidden px-4 py-3 font-medium sm:table-cell">Catégorie</th>
                <th className="hidden px-4 py-3 font-medium md:table-cell">Description</th>
                <th className="px-4 py-3 font-medium">Montant</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {[...depenses].sort((a, b) => new Date(b.date) - new Date(a.date)).map((d) => (
                <tr key={d.id} className="border-b border-slate-50 last:border-0 hover:bg-slate-50">
                  <td className="px-4 py-3 text-slate-600">{formatDate(d.date)}</td>
                  <td className="hidden px-4 py-3 sm:table-cell">
                    <span className="rounded-full bg-red-50 px-2 py-0.5 text-xs font-medium text-red-700">{d.categorie}</span>
                  </td>
                  <td className="hidden px-4 py-3 text-slate-500 md:table-cell">{d.description || "—"}</td>
                  <td className="px-4 py-3 font-semibold text-red-600">{formatCurrency(d.montant, settings.currency)}</td>
                  <td className="px-4 py-3 text-right">
                    <RowActions onEdit={() => onEdit(d)} onDelete={() => onDelete(d)} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function DepenseForm({ initial, onCancel, onSave }) {
  const [date, setDate] = useState(initial?.date || todayStr());
  const [montant, setMontant] = useState(initial?.montant ?? "");
  const [categorie, setCategorie] = useState(initial?.categorie || EXPENSE_CATEGORIES[0]);
  const [description, setDescription] = useState(initial?.description || "");
  const [error, setError] = useState("");

  const submit = (e) => {
    e.preventDefault();
    const amount = Number(montant);
    if (!date || !amount || amount <= 0) { setError("La date et un montant valide (supérieur à 0) sont requis."); return; }
    onSave({ date, montant: amount, categorie, description: description.trim() });
  };

  return (
    <Modal title={initial ? "Modifier la dépense" : "Ajouter une dépense"} onClose={onCancel}>
      <form onSubmit={submit}>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Date" required>
            <input type="date" className={inputClass()} value={date} onChange={(e) => setDate(e.target.value)} />
          </Field>
          <Field label="Montant" required>
            <input type="number" step="0.01" min="0" className={inputClass()} value={montant} onChange={(e) => setMontant(e.target.value)} placeholder="0.00" />
          </Field>
        </div>
        <Field label="Catégorie">
          <select className={inputClass()} value={categorie} onChange={(e) => setCategorie(e.target.value)}>
            {EXPENSE_CATEGORIES.map((c) => <option key={c} value={c}>{c}</option>)}
          </select>
        </Field>
        <Field label="Description">
          <input className={inputClass()} value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Ex. Facture fournisseur" />
        </Field>
        {error && <p className="mb-3 text-xs text-red-600">{error}</p>}
        <FormActions onCancel={onCancel} />
      </form>
    </Modal>
  );
}

/* ---------- Paramètres ---------- */

function ParametresView({ settings, onSave, onResetDemo, onClearAll }) {
  const [companyName, setCompanyName] = useState(settings.companyName);
  const [currency, setCurrency] = useState(settings.currency);

  const submit = (e) => {
    e.preventDefault();
    onSave({ companyName: companyName.trim() || "Mon entreprise", currency });
  };

  return (
    <div className="max-w-xl">
      <ViewHeader title="Paramètres" subtitle="Informations générales et données" />

      <form onSubmit={submit} className="rounded-xl border border-slate-200 bg-white p-5">
        <Field label="Nom de l'entreprise">
          <input className={inputClass()} value={companyName} onChange={(e) => setCompanyName(e.target.value)} />
        </Field>
        <Field label="Devise">
          <select className={inputClass()} value={currency} onChange={(e) => setCurrency(e.target.value)}>
            <option value="CAD">Dollar canadien (CAD)</option>
            <option value="USD">Dollar américain (USD)</option>
            <option value="EUR">Euro (EUR)</option>
          </select>
        </Field>
        <button type="submit" className="mt-1 rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700">
          Enregistrer
        </button>
      </form>

      <div className="mt-5 rounded-xl border border-slate-200 bg-white p-5">
        <p className="text-sm font-semibold text-slate-700">Données de démonstration</p>
        <p className="mt-1 text-sm text-slate-500">Cette version est un premier prototype fonctionnel. Les données ci-dessous restent dans votre navigateur.</p>
        <div className="mt-3 flex flex-wrap gap-2">
          <button onClick={onResetDemo} className="inline-flex items-center gap-1.5 rounded-md border border-slate-200 px-3 py-2 text-sm font-medium text-slate-600 hover:bg-slate-50">
            <RotateCcw size={15} /> Réinitialiser les données de démo
          </button>
          <button onClick={onClearAll} className="inline-flex items-center gap-1.5 rounded-md border border-red-200 px-3 py-2 text-sm font-medium text-red-600 hover:bg-red-50">
            <Trash2 size={15} /> Tout effacer
          </button>
        </div>
      </div>
    </div>
  );
}

/* ---------- Shared small components ---------- */

function ViewHeader({ title, subtitle, actionLabel, onAction }) {
  return (
    <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 className="text-lg font-semibold text-slate-900">{title}</h1>
        {subtitle && <p className="text-sm text-slate-500">{subtitle}</p>}
      </div>
      {actionLabel && (
        <button onClick={onAction} className="inline-flex items-center gap-1.5 rounded-md bg-emerald-600 px-3.5 py-2 text-sm font-medium text-white hover:bg-emerald-700">
          <Plus size={16} /> {actionLabel}
        </button>
      )}
    </div>
  );
}

function SearchBar({ value, onChange, placeholder }) {
  return (
    <div className="relative mb-4 max-w-sm">
      <Search size={16} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full rounded-md border border-slate-200 bg-white py-2 pl-9 pr-3 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500"
      />
    </div>
  );
}

function RowActions({ onEdit, onDelete }) {
  return (
    <div className="inline-flex items-center gap-1">
      <button onClick={onEdit} className="rounded p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-700" aria-label="Modifier">
        <Pencil size={15} />
      </button>
      <button onClick={onDelete} className="rounded p-1.5 text-slate-400 hover:bg-red-50 hover:text-red-600" aria-label="Supprimer">
        <Trash2 size={15} />
      </button>
    </div>
  );
}

function FormActions({ onCancel }) {
  return (
    <div className="mt-4 flex justify-end gap-2 border-t border-slate-100 pt-4">
      <button type="button" onClick={onCancel} className="rounded-md px-3.5 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100">
        Annuler
      </button>
      <button type="submit" className="rounded-md bg-emerald-600 px-3.5 py-2 text-sm font-medium text-white hover:bg-emerald-700">
        Enregistrer
      </button>
    </div>
  );
}

function inputClass(hasError) {
  return `w-full rounded-md border ${hasError ? "border-red-300" : "border-slate-200"} bg-white px-3 py-2 text-sm outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500`;
}
