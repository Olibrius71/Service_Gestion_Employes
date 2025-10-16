# Collection Bruno - SGE API

Collection de requêtes pour tester l'API du Service de Gestion des Employés.

## 📦 Installation

1. Installez Bruno : https://www.usebruno.com/
2. Ouvrez Bruno
3. Cliquez sur "Open Collection"
4. Sélectionnez le dossier `bruno-collection`

## 🚀 Utilisation

### Variables d'environnement
- **Local** : `http://localhost:8005` (défaut)

### Structure des dossiers

```
bruno-collection/
├── Departments/          # 5 endpoints pour les départements
│   ├── 1-List-All-Departments.bru
│   ├── 2-Get-Department-By-Id.bru
│   ├── 3-Create-Department.bru
│   ├── 4-Update-Department.bru
│   └── 5-Delete-Department.bru
├── Employees/            # 6 endpoints pour les employés
│   ├── 1-List-All-Employees.bru
│   ├── 2-List-Employees-Paginated.bru
│   ├── 3-Get-Employee-By-Id.bru
│   ├── 4-Create-Employee.bru
│   ├── 5-Update-Employee.bru
│   └── 6-Delete-Employee.bru
└── environments/
    └── Local.bru         # Variables d'environnement
```

## 🧪 Ordre de test recommandé

### Departments
1. **List All** → Voir les départements existants
2. **Create** → Créer un nouveau département (Finance, IT, HR...)
3. **Get By Id** → Récupérer un département spécifique
4. **Update** → Modifier un département
5. **Delete** → Supprimer un département

### Employees
1. **Create Department** d'abord (au moins un)
2. **Create Employee** → Utiliser un `departmentId` existant
3. **List All** → Voir tous les employés
4. **List Paginated** → Tester la pagination
5. **Get By Id** → Récupérer avec les infos du département
6. **Update** → Modifier partiellement un employé
7. **Delete** → Supprimer un employé

## 💡 Notes importantes

- Changez les IDs dans les URLs selon vos données
- Le département avec `departmentId: 3` existe déjà (IT Department)
- Les emails doivent être uniques
- Les codes de département doivent être uniques
- La pagination commence à pageIndex=1

## 🔥 Endpoints disponibles

### Departments
- `GET /api/departments` - Liste tous
- `GET /api/departments/{id}` - Par ID
- `POST /api/departments` - Créer
- `PUT /api/departments/{id}` - Modifier
- `DELETE /api/departments/{id}` - Supprimer

### Employees
- `GET /api/employees` - Liste tous
- `GET /api/employees?pageIndex=1&pageSize=5` - Pagination
- `GET /api/employees/{id}` - Par ID (avec département)
- `POST /api/employees` - Créer
- `PUT /api/employees/{id}` - Modifier (partiel)
- `DELETE /api/employees/{id}` - Supprimer


