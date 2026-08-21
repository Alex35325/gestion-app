# Gestion App

Prototype d'application de gestion financière pour petite entreprise, construit en React (composant unique `gestion-app_3.jsx`).

## Fonctionnalités

- **Tableau de bord** : revenus, dépenses, bénéfice net et marge, avec graphique des 6 derniers mois et activité récente.
- **Clients** : ajout, modification, suppression et recherche.
- **Revenus** : suivi des entrées par client, catégorie et montant.
- **Dépenses** : suivi par catégorie et montant.
- **Paramètres** : nom de l'entreprise, devise (CAD/USD/EUR), réinitialisation ou effacement des données de démonstration.

## Stack technique

- [React](https://react.dev/) (hooks : `useState`, `useEffect`, `useMemo`, `useCallback`)
- [lucide-react](https://lucide.dev/) pour les icônes
- [Recharts](https://recharts.org/) pour le graphique revenus/dépenses
- Tailwind CSS pour le style

## Données

Les données (clients, revenus, dépenses, paramètres) sont persistées via une API `window.storage` fournie par l'environnement hôte. Au premier chargement, si aucune donnée n'existe, des données de démonstration sont générées automatiquement.

## Statut

Premier prototype fonctionnel — les données restent dans le navigateur.
